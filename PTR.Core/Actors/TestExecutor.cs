using System;
using System.IO;
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
				Console.WriteLine("Assembly {0} to run with {1} fixtures received", Path.GetFileNameWithoutExtension(msg.Assembly), msg.TestFixtures.Length);
				if (_busy)
				{
					Sender.Tell(Busy.Instance);
					return;
				}

				_busy = true;
				var sender = Sender;
				var self = Self;
				var reporter = new RemoteReporter(msg.ReporterActor);

				System.Threading.Tasks.Task.Run(() =>
				{
					Runner.Run(msg, reporter);
					_busy = false;

					sender.Tell(TaskCompleted.Instance, self);
				});
			});

			Receive<TaskIsReady>(msg =>
			{
				if (_busy)
				{
					Sender.Tell(Busy.Instance);
				}
				else
				{
					Sender.Tell(RequestTask.Instance);
				}
			});

			Receive<NoTask>(msg =>
			{
				if (_busy)
				{
					Sender.Tell(Busy.Instance);
				}
				else
				{
					Sender.Tell(Bye.Instance);
				}
			});

			Receive<Greet>(msg =>
			{
				Console.WriteLine(msg.Message);
			});
		}
	}
}