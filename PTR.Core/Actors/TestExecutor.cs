using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using PTR.Core.NUnit;

namespace PTR.Core.Actors
{
	public class TestExecutor : ReceiveActor
	{
		protected ILoggingAdapter Log = Context.GetLogger();
		private bool _busy;

		public TestExecutor()
		{
			Become(Idle);
		}

		private void Idle()
		{
			Receive<Job>(msg =>
			{
				if (_busy)
				{
					Sender.Tell(Busy.Instance);
					return;
				}

				var sender = Sender;
				var self = Self;

				Task.Run(() =>
				{
					_busy = true;
					Runner.Run(msg);
					_busy = false;

					sender.Tell(JobCompleted.Instance, self);
				});
			});

			Receive<JobIsReady>(msg =>
			{
				Sender.Tell(RequestJob.Instance);
			});

			Receive<NoJob>(msg =>
			{
				Console.WriteLine((string) "NO JOB FOR ME. SO I WILL DIE {0}", (object) Self.Path.Name);
				Sender.Tell(Bye.Instance);
			});
		}

		public override void AroundPostStop()
		{
			var x = 0;
		}
	}
}