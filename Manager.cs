using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.Routing;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	public class Manager : ReceiveActor
	{
		private ConcurrentQueue<TestRun> _jobs = new ConcurrentQueue<TestRun>();

		public Manager()
		{
			var workers = new List<string>();

			Func<IActorRef> createRouter = () =>
			{
				// use router with round robin strategy
				return Context.ActorOf(Props.Empty.WithRouter(
					new RoundRobinGroup(workers.ToArray()))
					);
			};

			var router = createRouter();

			Receive<RegisterWorker>(input =>
			{
				Console.WriteLine("New worker {0}", input.Worker.PathString);

				workers.Add(input.Worker.PathString);
				router = createRouter();

				if (_jobs.Count > 0)
				{
					var assembly = Path.Combine(Environment.CurrentDirectory, "tests.dll");
					Console.WriteLine("Please run {0}", assembly);
					Self.Tell(new TestRun(assembly));
				}

//				while (_jobs.Count > 0)
//				{
//					TestRun job;
//					if (!_jobs.TryDequeue(out job)) break;
//					Run(job, router);
//				}
			});

			Receive<TestRun>(input =>
			{
				if (workers.Count == 0)
					_jobs.Enqueue(input);
				else
					Run(input, router);
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

			Receive<SuiteReport>(report =>
			{
				// TODO teamcity reporter
				Console.WriteLine("Suite {0} completed on '{1}'. Failed {2}. Passed {3}.",
					report.Suite, report.Agent, report.Failed, report.Passed);
			});
		}

		private void Run(TestRun input, IActorRef router)
		{
			var jobs = LoadTestFixtures(input.Assembly);
			foreach (var job in jobs)
			{
				router.Tell(job, Self);
			}
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