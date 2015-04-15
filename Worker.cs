using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Runs single test fixture.
	/// </summary>
	public class Worker : ReceiveActor
	{
		public Worker()
		{
			ActorSelection manager = null;

			Receive<SetManager>(set =>
			{
				Console.WriteLine("Connecting to master");
				manager = set.Manager;
				manager.Tell(new RegisterWorker(Context.ActorSelection(Self.Path)));
			});

			Receive<Job>(async input =>
			{
				// TODO download artifacts and save to temp folder
				Console.WriteLine("Downloading artifacts {0}", input.ArtifactsUrl);

				Console.WriteLine("Running test fixture {0} from {1}", input.Fixture, input.Assembly);

				var agentName = Self.Path.Name; // ToStringWithAddress();

				if (manager == null)
				{
					// TODO fail
					manager = new ActorSelection(Context.ActorOf<Manager>(), "manager");
				}

				using (var engine = TestEngineActivator.CreateInstance())
				{
					var package = new TestPackage(new[] {input.Assembly}); // TODO now assuming assembly path
					package.Settings["ProcessModel"] = "Single";
					package.Settings["DomainUsage"] = "None";

					var builder = new TestFilterBuilder
					{
						// TODO deal with fixture filter, it seems we need to send all tests from test fixture
						Tests = {input.Fixture}
					};
					
					var filter = builder.GetFilter();

					var listener = new RemoteReporter(Self, manager, agentName);

					using (var runner = engine.GetRunner(package))
					{
						var results = XElement.Load(new XmlNodeReader(runner.Run(listener, filter)));
						// Console.WriteLine(results.ToString());
					}
				}
			});
		}
	}
}
