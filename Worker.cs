using System;
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
			Receive<Job>(input =>
			{
				// TODO download artifacts and save to temp folder
				Console.WriteLine("Downloading artifacts {0}", input.ArtifactsUrl);

				Console.WriteLine("Running test fixture {0} from {1}", input.Fixture, input.Assembly);

				var manager = Context.ActorOf<Manager>();
				var agentName = Self.Path.Name; // ToStringWithAddress();

				using (var engine = TestEngineActivator.CreateInstance())
				{
					var package = new TestPackage(new[] {input.Assembly}); // TODO now assuming assembly path
					package.Settings["ProcessModel"] = "Single";

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
