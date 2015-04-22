using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
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
	public sealed class Worker : ReceiveActor
	{
		protected ILoggingAdapter Log = Context.GetLogger();
		private readonly HashSet<IActorRef> _masters = new HashSet<IActorRef>();
		private bool _busy;

		public Worker()
		{
			Become(Idle);
		}

		private void Idle()
		{
			Receive<SetMaster>(msg =>
			{
				Log.Info("Setting new master {0} for worker {1}", msg.Master.PathString, Self.Path.Name);
				msg.Master.Tell(Greet.Instance, Self);
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
				if (_busy)
				{
					Sender.Tell(Busy.Instance);
					return;
				}

				Sender.Tell(RequestJob.Instance);
			});

			Receive<Job>(job =>
			{
				_busy = true;

				Log.Info("Downloading artifacts {0}", job.ArtifactsUrl);
				Log.Info("Running test fixture {0} from {1}", job.TestFixture, job.Assembly);

				var sender = Sender;
				var self = Self;

				Task.Run(() =>
				{
					RunTests(job, sender, self);

					_busy = false;

					sender.Tell(new JobCompleted(), self);
				});
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

		private static TestResult RunTests(Job job, IActorRef sender, IActorRef self)
		{
			ServiceManager.Services.AddService(new DomainManager());
			ServiceManager.Services.AddService(new ProjectService());
			ServiceManager.Services.AddService(new TestAgency());
			ServiceManager.Services.InitializeServices();

			var assemblyDir = Path.GetDirectoryName(job.Assembly);

			var testPackage = new TestPackage(job.Assembly);
			testPackage.Settings["ProcessModel"] = ProcessModel.Single;
			testPackage.Settings["DomainUsage"] = DomainUsage.Single;
			testPackage.Settings["ShadowCopyFiles"] = false;
			testPackage.Settings["WorkDirectory"] = assemblyDir;

			// TODO enable config injection if really needed
//			var configPath = new FileInfo(Path.Combine(assemblyDir, Path.GetFileName(job.Assembly) + ".config"));
//			var configMap = new ExeConfigurationFileMap {ExeConfigFilename = configPath.FullName};
//			var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
//
//			foreach (var key in config.AppSettings.Settings.AllKeys)
//			{
//				ConfigurationManager.AppSettings[key] = config.AppSettings.Settings[key].Value;
//			}
			
			var outWriter = Console.Out;
			var errorWriter = Console.Error;
			var olddir = Environment.CurrentDirectory;

			try
			{
				var listener = new CompositeEventListener(new EventListener[]
				{
					new EventListenerImpl(self.Path.Name, e => sender.Tell(e, self)),
					new NUnitEventListener(outWriter, errorWriter)
				});

				var filter = new SimpleNameFilter(job.TestFixture);

				using (var runner = new DefaultTestRunnerFactory().MakeTestRunner(testPackage))
				{
					runner.Load(testPackage);
					var result = runner.Run(listener, filter, true, LoggingThreshold.All);
					return result;
				}
			}
			finally
			{
				Environment.CurrentDirectory = olddir;
				outWriter.Flush();
				errorWriter.Flush();

				Console.SetOut(outWriter);
				Console.SetError(errorWriter);
			}
		}
	}
}
