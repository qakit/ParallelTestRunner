using System;
using NUnit.Core;

namespace PTR.Core.NUnit
{
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
		public double Duration { get; set; }

		// valid for finished events
		public Exception Error { get; set; }

		// valid for started events
		public string FullName { get; set; }
	}
}