using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.NUnit.Runtime.Messages;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	public class Master : ReceiveActor
	{
		private readonly HashSet<IActorRef> _workers = new HashSet<IActorRef>();
		private readonly Queue<Job> _workQueue = new Queue<Job>();
		private List<RunningTest> _runningTests = new List<RunningTest>(); 

		public Master()
		{
			Receive<RegisterWorker>(newWorker =>
			{
				Console.WriteLine("Created new worker {0}", newWorker.Worker.Path.Name);
				Context.Watch(newWorker.Worker);
				_workers.Add(newWorker.Worker);
				if (_workQueue.Count > 0)
				{
					newWorker.Worker.Tell(new WorkIsReady());
				}
			});

			Receive<RequestWork>(request =>
			{
				var worker = Sender;
				Console.WriteLine("Worker {0} requests for a work", worker.Path.Name);

				//TODO handle no work to be done situation;
				//TODO load tests somehwere else not here
				if (_workQueue.Count == 0)
				{
					worker.Tell(new NoWorkToBeDone());
				}
				else
				{
					var task = _workQueue.Dequeue();
					var runnintTest = new RunningTest
					{
						Name = task.Fixture,
						ArtifactsUri = task.ArtifactsUrl,
						AssemblyPath = task.Assembly,
						Worker = worker
					};

					_runningTests.Add(runnintTest);
					worker.Tell(new WorkToBeDone(task), Self);
				}
			});

			Receive<TestRun>(input =>
			{
				var testFixtures = LoadTestFixtures(input.Assembly);
				foreach (Job work in testFixtures)
				{
					_workQueue.Enqueue(work);
				}
			});

			Receive<TestReport>(report =>
			{
				// TODO integrate teamcity reporter
				var status = report.Failed ? "FAILED" : "PASSED";
				Console.WriteLine("Test {0} is {1} on agent '{2}'.", report.Test, status, report.Agent);
				if (!string.IsNullOrEmpty(report.Output))
				{
					Console.WriteLine("Output:");
					Console.WriteLine(report.Output);
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
						let type = d.GetAttribute("type")
						let name = d.GetAttribute("fullname")
						where name != null && type != null && type == "TestFixture"
						select new Job(assemblyPath, name, artifactsUrl);
				}
			}
		}
	}
}