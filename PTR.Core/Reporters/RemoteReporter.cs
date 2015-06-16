using System;
using Akka.Actor;
using NUnit.Core;
using PTR.Core.NUnit;

namespace PTR.Core.Reporters
{
	internal class RemoteReporter : IReporter
	{
		private readonly IActorRef _reporterActor;

		public RemoteReporter(IActorRef reporterActor)
		{
			_reporterActor = reporterActor;
		}

		public void RunStarted(string name, int testCount)
		{
			_reporterActor.Tell(new RunStarted(name, testCount));
		}

		public void RunFinished(TestEvent result)
		{
			_reporterActor.Tell(new RunFinished(result));
		}

		public void RunFinished(Exception exception)
		{
			_reporterActor.Tell(new RunFinished(exception));
		}

		public void TestStarted(TestName testName)
		{
			_reporterActor.Tell(new TestStarted(testName));
		}

		public void TestFinished(TestEvent result)
		{
			_reporterActor.Tell(new TestFinished(result));
		}

		public void SuiteStarted(TestName testName)
		{
			_reporterActor.Tell(new SuiteStarted(testName));
		}

		public void SuiteFinished(TestEvent result)
		{
			_reporterActor.Tell(new SuiteFinished(result));
		}

		public void UnhandledException(Exception exception)
		{
			_reporterActor.Tell(new UnhandledException(exception));
		}

		public void TestOutput(global::NUnit.Core.TestOutput testOutput)
		{
			_reporterActor.Tell(new TestOutput(testOutput));
		}
	}
}