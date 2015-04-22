using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.NUnit.Runtime.Messages;
using Akka.NUnit.Runtime.Reporters;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;

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

		private TestResult RunTests(Job job)
		{
			ServiceManager.Services.AddService(new DomainManager());
			ServiceManager.Services.AddService(new TestAgency());
			ServiceManager.Services.InitializeServices();

			var testPackage = new TestPackage(job.Assembly);
			testPackage.Settings["ProcessModel"] = ProcessModel.Single;
			testPackage.Settings["DomainUsage"] = DomainUsage.None;
			testPackage.Settings["ShadowCopyFiles"] = false;
			testPackage.Settings["WorkDirectory"] = Path.GetDirectoryName(job.Assembly);

			var outWriter = Console.Out;
			var errorWriter = Console.Error;

			try
			{
				var self = Self;
				var sender = Sender;

				var listener = new CompositeEventListener(new EventListener[]
				{
					new EventListenerImpl(Self.Path.Name, e => sender.Tell(e, self)),
					new NUnitEventListener(outWriter, errorWriter)
				});

				var testFilter = new SimpleNameFilter(job.TestFixture);

				using (var testRunner = new DefaultTestRunnerFactory().MakeTestRunner(testPackage))
				{
					testRunner.Load(testPackage);
					var result = testRunner.Run(listener, testFilter, true, LoggingThreshold.All);
					return result;
				}
			}
			finally
			{
				outWriter.Flush();
				errorWriter.Flush();

				Console.SetOut(outWriter);
				Console.SetError(errorWriter);
			}
		}
	}
}
