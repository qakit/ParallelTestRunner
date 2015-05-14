using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.NUnit.Runtime.Messages;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Runs single test fixture.
	/// </summary>
	public class Worker : ReceiveActor
	{
		protected ILoggingAdapter Log = Context.GetLogger();
		private readonly HashSet<IActorRef> _masters = new HashSet<IActorRef>();
		private bool _busy;
        private string WorkingDir { get; set; }

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

		    Receive<SetWorkingDir>(msg =>
		    {
		        WorkingDir = msg.WorkingDir;
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
				if (_busy)
				{
					Sender.Tell(Busy.Instance, Self);
					return;
				}

				_busy = true;


                //TODO download artifacts here;
				Log.Info("Downloading artifacts {0}", job.ArtifactsUrl);
				Log.Info("Running test fixture {0} from {1}", job.TestFixture, job.Assembly);

				var sender = Sender;
				var self = Self;

				Task.Run(() =>
				{
					NUnit2Runner.Run(job, sender, self);

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
				master.Tell(Bye.Shutdown, Self);
			}
		}		
	}
}
