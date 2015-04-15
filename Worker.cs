using System;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.NUnit.Runtime.Messages;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Runs single test fixture.
	/// </summary>
	public class Worker : ReceiveActor
	{
		private ActorSelection _master;

		public Worker()
		{
			Become(Idle);
		}

		private void Idle()
		{
			Receive<SetMaster>(input =>
			{
				Console.WriteLine("Sender: {0}", Sender.Path);
				Console.WriteLine("Setting new master {0} for worker", input.Master.PathString);
				_master = input.Master;
				_master.Tell(new RegisterWorker(Self));
			});

			Receive<JobIsReady>(ready =>
			{
				Console.WriteLine("Worker requests for a work");
				_master.Tell(new RequestJob());
			});

			Receive<Job>(job =>
			{
				Console.WriteLine("Downloading artifacts {0}", job.ArtifactsUrl);
				Console.WriteLine("Running test fixture {0} from {1}", job.TestFixture, job.Assembly);

				using (var engine = TestEngineActivator.CreateInstance())
				{
					var package = new TestPackage(new[] { job.Assembly }); // TODO now assuming assembly path
					package.Settings["ProcessModel"] = "Single";
					package.Settings["DomainUsage"] = "None";

					var builder = new TestFilterBuilder
					{
						// TODO deal with fixture filter, it seems we need to send all tests from test fixture
						Tests = { job.TestFixture }
					};

					var filter = builder.GetFilter();

					var listener = new RemoteReporter(Self, Sender, Self.Path.Name);

					using (var runner = engine.GetRunner(package))
					{
						var results = XElement.Load(new XmlNodeReader(runner.Run(listener, filter)));
						Console.WriteLine(results.ToString());
					}
				}

				Sender.Tell(new SuiteReport(Self, job.TestFixture, 0, 0));

				_master.Tell(new RequestJob());
			});

			Receive<NoJob>(_ => { });
		}
	}
}
