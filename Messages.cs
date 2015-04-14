namespace Akka.NUnit.Runtime
{
	public class TestRun
	{
		public TestRun(string assembly)
		{
			Assembly = assembly;
		}

		public string Assembly { get; private set; }
	}

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

	public class SuiteReport
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

	public class Job
	{
		public Job(string assembly, string fixture, string artifactsUrl)
		{
			Assembly = assembly;
			Fixture = fixture;
			ArtifactsUrl = artifactsUrl;
		}

		public string Assembly { get; private set; }
		public string Fixture { get; private set; }
		public string ArtifactsUrl { get; private set; }
	}
}
