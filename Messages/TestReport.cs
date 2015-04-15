namespace Akka.NUnit.Runtime
{
	public class TestReport
	{
		public TestReport(string agent, string test, bool failed, string output)
		{
			Agent = agent;
			Test = test;
			Failed = failed;
			Output = output;
		}

		/// <summary>
		/// Name of worker who run the test.
		/// </summary>
		public string Agent { get; private set; }
		public string Test { get; private set; }
		public bool Failed { get; private set; }
		public string Output { get; private set; }

		// TODO add duration

		// TODO progress
		// public int Current { get; private set; }
		// public int Total { get; private set; }
	}
}