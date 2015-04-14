using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Routing;

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
				var status = report.Failed ? "FAILED" : "PASSED";
				Console.WriteLine("Test {0} is {1} on agent '{2}'", report.Test, status, report.Agent);
			});

			Receive<SuiteReport>(report =>
			{
				Console.WriteLine("Suite {0} completed on '{1}'. Failed {2}. Passed {3}.",
					report.Suite, report.Agent, report.Failed, report.Passed);
			});
		}

		private IEnumerable<Job> LoadTestFixtures(string assemblyPath)
		{
			// TODO load assembly and collect jobs

			var random = new Random();
			var artifactsUrl = "http://localhost/artifacts/9.9.9.9";

			return from i in Enumerable.Range(0, random.Next(10, 20))
				select new Job(assemblyPath, "Fixture" + (i + 1), artifactsUrl);
		}
	}
}