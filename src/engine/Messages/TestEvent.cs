using System;
using NUnit.Core;

namespace Akka.NUnit.Runtime.Messages
{
	public enum EventKind
	{
		RunStarted,
		RunFinished,
		TestStarted,
		TestFinishied,
		SuiteStarted,
		SuiteFinished
	}

	public sealed class TestEvent
	{
		/// <summary>
		/// Name of worker who run the test.
		/// </summary>
		public string Worker { get; set; }
		public EventKind Kind { get; set; }
		public int TestCount { get; set; }
		public ResultState Result { get; set; }
		public string Message { get; set; }
		public string StackTrace { get; set; }
		public double Time { get; set; }

		// valid for finished events
		public Exception Error { get; set; }

		// valid for started events
		public string FullName { get; set; }
	}
}