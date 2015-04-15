using Akka.Actor;

namespace Akka.NUnit.Runtime.Messages
{
	// TODO combine with test report

	public sealed class SuiteReport
	{
		public SuiteReport(IActorRef worker, string suite, int passed, int failed)
		{
			Worker = worker;
			Suite = suite;
			Passed = passed;
			Failed = failed;
		}

		/// <summary>
		/// Name of worker who run the test.
		/// </summary>
		public IActorRef Worker { get; private set; }
		public string Suite { get; private set; }
		public int Passed { get; private set; }
		public int Failed { get; private set; }

		// TODO progress
		// public int Current { get; private set; }
		// public int Total { get; private set; }
	}
}