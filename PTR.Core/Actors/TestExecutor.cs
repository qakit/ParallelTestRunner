using Akka.Actor;
using Akka.Event;
using PTR.Core.Messages;
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

					sender.Tell(TaskCompleted.Instance, self);
				});
			});

			Receive<TaskIsReady>(msg =>
			{
				Sender.Tell(RequestTask.Instance);
			});

			Receive<NoTask>(msg =>
			{
				Sender.Tell(Bye.Instance);
			});
		}
	}
}