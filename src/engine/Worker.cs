using System.Collections.Generic;
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
		private readonly HashSet<IActorRef> _masters = new HashSet<IActorRef>();

		public Worker()
		{
			Become(Idle);
		}

		private void Idle()
		{
			Receive<SetMaster>(msg =>
			{
				Log.Info("Setting new master {0} for worker {1}", msg.Master.PathString, Self.Path.Name);
				msg.Master.Tell(new Greet(), Self);
			});

			Receive<Greet>(_ =>
			{
				_masters.Add(Sender);
			});

			Receive<Bye>(msg =>
			{
				if (_masters.Remove(Sender))
				{
					Log.Info("Master {0} disconnected", Sender.Path);
				}
			});

			Receive<JobIsReady>(_ =>
			{
				Log.Debug("There are new jobs");
				Sender.Tell(new RequestJob());
			});

			Receive<Job>(job =>
			{
				Log.Info("Downloading artifacts {0}", job.ArtifactsUrl);
				Log.Info("Running test fixture {0} from {1}", job.TestFixture, job.Assembly);

				RunTests(job);
				
				Sender.Tell(new JobCompleted(), Self);
			});

			Receive<NoJob>(_ => { });

			Receive<Terminated>(t =>
			{
				Log.Info("Terminated {0}", t.ActorRef.Path);

				if (t.ActorRef == Self)
				{
					Unregister();
				}
			});

			Receive<PoisonPill>(_ => Unregister());
		}

		private void Unregister()
		{
			Log.Info("Unregistering worker {0}", Self.Path);

			foreach (var master in _masters)
			{
				master.Tell(new Bye("fire"), Self);
			}
		}

		private void RunTests(Job job)
		{
			using (var engine = TestEngineActivator.CreateInstance())
			{
				engine.Initialize();

				var package = new TestPackage(new[] {job.Assembly}); // TODO now assuming assembly path
				package.Settings["ProcessModel"] = "Single";
				package.Settings["DomainUsage"] = "None";

				var builder = new TestFilterBuilder
				{
					Tests = {job.TestFixture}
				};

				var filter = builder.GetFilter();

				var listener = new EventListener(Self.Path.Name, e => Sender.Tell(e, Self));

				using (var runner = engine.GetRunner(package))
				{
					var results = XElement.Load(new XmlNodeReader(runner.Run(listener, filter)));
					// TODO accumulate results if master is died
					// Console.WriteLine(results);
				}
			}
		}
	}
}
