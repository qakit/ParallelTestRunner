using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.NUnit.Runtime.Messages;
using Akka.NUnit.Runtime.Reporters;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Runs single test fixture.
	/// </summary>
	public class Worker : ReceiveActor
	{
		protected ILoggingAdapter Log = Context.GetLogger();
		private ActorSelection _master;

		public Worker()
		{
			Become(Idle);
		}

		private void Idle()
		{
			Receive<SetMaster>(input =>
			{
				Log.Debug("Sender: {0}", Sender.Path);
				Log.Info("Setting new master {0} for worker {1}", input.Master.PathString, Self.Path.Name);
				_master = input.Master;
				_master.Tell(new RegisterWorker(), Self);
			});

			Receive<JobIsReady>(ready =>
			{
				Log.Debug("There are new jobs");
				_master.Tell(new RequestJob());
			});

			Receive<Job>(job =>
			{
				Log.Info("Downloading artifacts {0}", job.ArtifactsUrl);
				Log.Info("Running test fixture {0} from {1}", job.TestFixture, job.Assembly);

				using (var engine = TestEngineActivator.CreateInstance())
				{
					var package = new TestPackage(new[] { job.Assembly }); // TODO now assuming assembly path
					package.Settings["ProcessModel"] = "Single";
					package.Settings["DomainUsage"] = "None";

					var builder = new TestFilterBuilder
					{
						Tests = { job.TestFixture }
					};

					var filter = builder.GetFilter();

					var listener = new EventListener(Self.Path.Name, e => Sender.Tell(e, Self));

					using (var runner = engine.GetRunner(package))
					{
						var results = XElement.Load(new XmlNodeReader(runner.Run(listener, filter)));
					}
				}
				
				_master.Tell(new JobCompleted(), Self);
			});

			Receive<NoJob>(_ => { });
		}
	}
}
