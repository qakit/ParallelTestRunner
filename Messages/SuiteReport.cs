namespace Akka.NUnit.Runtime.Messages
{
	// TODO combine with test report

	public sealed class SuiteReport
	{
		public SuiteReport(string agent, string suite, int passed, int failed)
		{
			Agent = agent;
			Suite = suite;
			Passed = passed;
			Failed = failed;
		}

		/// <summary>
		/// Name of worker who run the test.
		/// </summary>
		public string Agent { get; private set; }
		public string Suite { get; private set; }
		public int Passed { get; private set; }
		public int Failed { get; private set; }

		// TODO progress
		// public int Current { get; private set; }
		// public int Total { get; private set; }
	}
}