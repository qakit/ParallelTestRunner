using Akka.Actor;
using PTR.Core.Reporters;

namespace PTR.Core.Actors
{
	public class TestReporter : ReceiveActor
	{
		public IReporter Reporter { get; set; }

		public TestReporter()
		{
			Become(Idle);
		}

		public void Idle()
		{
			Receive<SetReporter>(msg =>
			{
				Reporter = msg.Reporter;
			});

			Receive<RunStarted>(msg =>
			{
				Reporter.RunStarted(msg.Name, msg.TestCount);
			});

			Receive<RunFinished>(msg =>
			{
				if (msg.Exception != null)
					Reporter.RunFinished(msg.Exception);
				else
					Reporter.RunFinished(msg.Result);
			});

			Receive<TestStarted>(msg =>
			{
				Reporter.TestStarted(msg.TestName);
			});

			Receive<TestFinished>(msg =>
			{
				Reporter.TestFinished(msg.Result);
			});

			Receive<SuiteStarted>(msg =>
			{
				Reporter.SuiteStarted(msg.TestName);
			});

			Receive<SuiteFinished>(msg =>
			{
				Reporter.SuiteFinished(msg.Result);
			});

			Receive<UnhandledException>(msg =>
			{
				Reporter.UnhandledException(msg.Exception);
			});

			Receive<TestOutput>(msg =>
			{
				Reporter.TestOutput(msg.Output);
			});
		}
	}
}
