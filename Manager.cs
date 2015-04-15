using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.NUnit.Runtime.Messages;
using NUnit.Engine;
using TestRun = Akka.NUnit.Runtime.Messages.TestRun;

namespace Akka.NUnit.Runtime
{
	public sealed class Manager : ReceiveActor
	{
		private readonly HashSet<IActorRef> _workers = new HashSet<IActorRef>();
		private readonly ConcurrentQueue<Job> _jobQueue = new ConcurrentQueue<Job>();

		// TODO consider to remove, is it needed?
		private readonly List<RunningTest> _runningTests = new List<RunningTest>(); 

		public Manager()
		{
			Receive<RegisterWorker>(_ =>
			{
				Console.WriteLine("Created new worker {0}", Sender.Path.Name);

				Context.Watch(Sender);

				_workers.Add(Sender);

				if (_jobQueue.Count > 0)
				{
					Sender.Tell(new JobIsReady());
				}
			});

			Receive<RequestJob>(request =>
			{
				var worker = Sender;
				Console.WriteLine("Worker {0} requests for a work", worker.Path.Name);

				//TODO handle no work to be done situation;
				//TODO load tests somehwere else not here

				Job job;
				if (_jobQueue.Count > 0 && _jobQueue.TryDequeue(out job))
				{
					var runnintTest = new RunningTest
					{
						Name = job.TestFixture,
						ArtifactsUri = job.ArtifactsUrl,
						AssemblyPath = job.Assembly,
						Worker = worker
					};

					_runningTests.Add(runnintTest);
					worker.Tell(job, Self);
				}
				else
				{
					worker.Tell(new NoJob());
				}
			});

			Receive<JobCompleted>(_ =>
			{
				if (_jobQueue.Count > 0)
				{
					Sender.Tell(new JobIsReady());
				}
			});

			Receive<TestRun>(input =>
			{
				Console.WriteLine("New test run for {0}", input.Assembly);

				var testFixtures = LoadTestFixtures(input.Assembly);
				foreach (Job work in testFixtures)
				{
					_jobQueue.Enqueue(work);
				}
			});

			Receive<TestEvent>(e =>
			{
				// TODO integrate teamcity reporter
				Console.WriteLine("{0} {1} is {2} by '{3}'.", e.Kind, e.FullName, e.Result, e.Worker);

				if (!string.IsNullOrEmpty(e.Output))
				{
					Console.WriteLine("Output:");
					Console.WriteLine(e.Output);
				}
			});
		}

		private IEnumerable<Job> LoadTestFixtures(string assemblyPath)
		{
			// TODO zip package with testing assemblies
			// TODO copy zip package to HTTP server dir

			var artifactsUrl = assemblyPath; // TODO URL to http server

			using (var engine = TestEngineActivator.CreateInstance())
			{
				var package = new TestPackage(new[] {assemblyPath});
				package.Settings["ProcessModel"] = "Single";
				var builder = new TestFilterBuilder();
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