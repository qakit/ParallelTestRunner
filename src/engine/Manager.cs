using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akka.Event;
using Akka.NUnit.Runtime.Messages;
using Akka.NUnit.Runtime.Reporters;
using NUnit.Framework;

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
			Receive<Greet>(_ =>
			{
				Log.Info("New worker {0}", Sender.Path.Name);

				Context.Watch(Sender);
				_workers.Add(Sender);

				Sender.Tell(new Greet(), Self);

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

			Receive<RunTests>(msg =>
			{
				Log.Info("New test run of assembly {0}", msg.Assembly);

				var testFixtures = LoadTestFixtures(msg);
				foreach (var job in testFixtures)
				{
					_jobQueue.Enqueue(job);
				}

				NotifyJobIsReady();
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
				RemoveWorker(worker);
			});

			Receive<Terminated>(t =>
			{
				if (t.ActorRef == Self)
				{
					Stop();
				}
				else
				{
					RemoveWorker(t.ActorRef);
				}
			});

			Receive<PoisonPill>(_ => Stop());
		}

		private void Stop()
		{
			Log.Info("Killing manager");

			foreach (var worker in _workers)
			{
				worker.Tell(new Bye("night"));
			}
		}

		private void NotifyJobIsReady()
		{
			if (_jobQueue.Count <= 0) return;

			foreach (var worker in _workers)
			{
				worker.Tell(new JobIsReady(), Self);
			}
		}

		private void RemoveWorker(IActorRef worker)
		{
			if (!_workers.Remove(worker)) return;
			Log.Info("Killing worker {0}", worker.Path.Name);
			ReaddTaskIfAny(worker);
		}

		private void ReaddTaskIfAny(IActorRef worker)
		{
			var task = _runningJobs.FirstOrDefault(job => job.Worker.Equals(worker));
			if (task != null)
			{
				_jobQueue.Enqueue(task.Job);
				NotifyJobIsReady();
			}
		}

		private IEnumerable<Job> LoadTestFixtures(RunTests run)
		{
			var assembly = Assembly.LoadFrom(run.Assembly);

			var fixtures = from t in assembly.GetTypes()
				where t.HasAttribute<TestFixtureAttribute>()
				let cat = t.GetAttribute<CategoryAttribute>().IfNotNull(a => a.Name) ?? string.Empty
				where (run.Include.Length == 0 || run.Include.Any(s => s == cat)) && run.Exclude.All(s => s != cat)
				select t;

			// TODO zip package with testing assemblies
			// TODO copy zip package to HTTP server dir

			return (from type in fixtures select new Job(run.Assembly, type.FullName, GetTests(type), run.Assembly)).ToList();
		}

		private static string[] GetTests(Type type)
		{
			var listOfTestsInLibrary = (from method in type.GetMethods()
				where method.HasAttribute<TestAttribute>() ||
				      method.HasAttribute<TestCaseAttribute>() ||
				      method.HasAttribute<TestCaseSourceAttribute>()
				select type.FullName + "." + method.Name).ToArray();
			return listOfTestsInLibrary;
		}
	}

	internal static class ReflectionExt
	{
		public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T:Attribute
		{
			var attrs = (T[])provider.GetCustomAttributes(typeof (T), inherit);
			return attrs.FirstOrDefault();
		}

		public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			return provider.GetAttribute<T>() != null;
		}
	}
}