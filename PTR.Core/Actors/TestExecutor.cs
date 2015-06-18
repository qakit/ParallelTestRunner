using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using PTR.Core.NUnit;
using PTR.Core.Reporters;

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
			Receive<Greet>(msg =>
			{
				Console.WriteLine(msg.Message);
			});

			Receive<Task>(msg =>
			{
				if (_busy)
				{
					Sender.Tell(Busy.Instance);
					return;
				}

				var sender = Sender;
				var self = Self;
				var reporter = new RemoteReporter(msg.ReporterActor);

				System.Threading.Tasks.Task.Run(() =>
				{
					_busy = true;
					Runner.Run(msg, reporter);
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
				Sender.Tell(Bye.Instance);
			});
		}
	}
}