using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.NUnit.Runtime.Messages;
using Akka.NUnit.Runtime.Reporters;
using NUnit.Engine;
using TestRun = Akka.NUnit.Runtime.Messages.TestRun;

namespace Akka.NUnit.Runtime
{
	public class Manager : ReceiveActor
	{
		private class RunningJob
		{
			public readonly IActorRef Worker;
			public readonly Job Job;

			public RunningJob(IActorRef worker, Job job)
			{
				Worker = worker;
				Job = job;
			}
		}

		protected ILoggingAdapter Log = Context.GetLogger();
		private readonly HashSet<IActorRef> _workers = new HashSet<IActorRef>();
		private readonly ConcurrentQueue<Job> _jobQueue = new ConcurrentQueue<Job>();
		private readonly List<RunningJob> _runningJobs = new List<RunningJob>(); 

		public Manager()
		{
			Receive<RegisterWorker>(_ =>
			{
				Log.Info("New worker {0}", Sender.Path.Name);

				Context.Watch(Sender);
				_workers.Add(Sender);

				if (_jobQueue.Count > 0)
				{
					Sender.Tell(new JobIsReady(), Self);
				}
			});

			Receive<RequestJob>(request =>
			{
				var worker = Sender;
				Log.Debug("Worker {0} requests job", worker.Path.Name);

				//TODO handle no work to be done situation;
				//TODO load tests somehwere else not here

				Job job;
				if (_jobQueue.Count > 0 && _jobQueue.TryDequeue(out job))
				{
					_runningJobs.Add(new RunningJob(worker, job));
					worker.Tell(job, Self);
				}
				else
				{
					worker.Tell(new NoJob(), Self);
				}
			});

			Receive<JobCompleted>(_ =>
			{
				_runningJobs.RemoveAll(job => job.Worker.Equals(Sender));

				if (_jobQueue.Count > 0)
				{
					Sender.Tell(new JobIsReady(), Self);
				}
			});

			Receive<TestRun>(msg =>
			{
				Log.Info("New test run of assembly {0}", msg.Assembly);

				var testFixtures = LoadTestFixtures(msg);
				foreach (var job in testFixtures)
				{
					_jobQueue.Enqueue(job);
				}

				if (_jobQueue.Count > 0)
				{
					foreach (var worker in _workers)
					{
						worker.Tell(new JobIsReady(), Self);
					}
				}
			});

			Receive<TestEvent>(e =>
			{
				// TODO integrate teamcity reporter
				Log.Info("{0} {1} is {2} by '{3}'.", e.Kind, e.FullName, e.Result, e.Worker);

				var reporter = new TeamCityReporter(Console.Out);
				reporter.Report(e);

				if (!string.IsNullOrEmpty(e.Output))
				{
					Log.Info("Output:");
					Log.Info(e.Output);
				}
			});

			Receive<Bye>(msg =>
			{
				var worker = Sender;
				if (_workers.Remove(worker))
				{
					Log.Info("Killing worker {0}", worker.Path.Name);
					ReaddTaskIfAny(worker);
				}
			});

			Receive<Terminated>(t =>
			{
				if (t.ActorRef == Self)
				{
					Console.WriteLine("master is shutdown");

					foreach (var worker in _workers)
					{
						worker.Tell(new Bye("night"));
					}
				}
				else
				{
					var worker = t.ActorRef;
					if (_workers.Remove(worker))
					{
						Log.Info("Killing worker {0}", worker.Path.Name);
						ReaddTaskIfAny(worker);
					}
				}
			});

			ReceiveAny(any =>
			{
				//readd task on failure to the queue
				var runningJob = any as RunningJob;
				if (runningJob != null)
				{
					_jobQueue.Enqueue(runningJob.Job);
				}
			});
		}

		private void ReaddTaskIfAny(IActorRef worker)
		{
			var task = _runningJobs.FirstOrDefault(job => job.Worker.Equals(worker));
			if (task != null)
			{
				Self.Tell(task, Sender);
			}
		}

		private IEnumerable<Job> LoadTestFixtures(TestRun run)
		{
			// TODO zip package with testing assemblies
			// TODO copy zip package to HTTP server dir

			var assemblyPath = run.Assembly;
			var artifactsUrl = assemblyPath; // TODO URL to http server

			using (var engine = TestEngineActivator.CreateInstance())
			{
				var package = new TestPackage(new[] {assemblyPath});
				package.Settings["ProcessModel"] = "Single";
				var builder = new TestFilterBuilder(null, run.Include, run.Exclude);
				var filter = builder.GetFilter();

				using (var runner = engine.GetRunner(package))
				{
					var tests = XElement.Load(new XmlNodeReader(runner.Explore(filter)));

					return from d in tests.Descendants()
						let type = d.GetAttribute("type", (string) null)
						let name = d.GetAttribute("fullname", (string) null)
						where name != null && type != null && type == "TestFixture"
						select new Job(assemblyPath, name, artifactsUrl);
				}
			}
		}
	}
}