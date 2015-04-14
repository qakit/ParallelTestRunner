using System;
using Akka.Actor;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Runs single test fixture.
	/// </summary>
	public class Worker : ReceiveActor
	{
		public Worker()
		{
			Receive<Job>(input =>
			{
				// TODO download artifacts and save to temp folder
				Console.WriteLine("Downloading artifacts {0}", input.ArtifactsUrl);

				Console.WriteLine("Running test fixture {0} from {1}", input.Fixture, input.Assembly);

				var manager = Context.ActorOf<Manager>();
				var agentName = Self.Path.Name; // ToStringWithAddress();

				var random = new Random();
				var passCount = 0;
				var failCount = 0;
				
				for (var i = 0; i < random.Next(10, 100); i++)
				{
					var failed = random.NextDouble() >= 0.5;
					if (failed) failCount++;
					else passCount++;

					manager.Tell(new TestReport(agentName, "Test" + (i + 1), failed, failed ? "ops" : null), Self);
				}

				manager.Tell(new SuiteReport(agentName, input.Fixture, passCount, failCount));
			});
		}
	}
}
