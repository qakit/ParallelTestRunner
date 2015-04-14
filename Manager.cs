using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.Routing;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	public class Manager : ReceiveActor
	{
		public Manager()
		{
			var workers = Enumerable.Range(1, 4).Select(i => "user/worker" + i).ToArray();

			// use router with round robin strategy
			var router = Context.ActorOf(Props.Empty.WithRouter(
				new RoundRobinGroup(workers))
				);

			Receive<TestRun>(input =>
			{
				foreach (var job in LoadTestFixtures(input.Assembly))
				{
					router.Tell(job, Self);
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

			Receive<SuiteReport>(report =>
			{
				// TODO teamcity reporter
				Console.WriteLine("Suite {0} completed on '{1}'. Failed {2}. Passed {3}.",
					report.Suite, report.Agent, report.Failed, report.Passed);
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