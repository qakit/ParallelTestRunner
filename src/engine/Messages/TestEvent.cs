using System;

namespace Akka.NUnit.Runtime.Messages
{
	public sealed class TestEvent
	{
		/// <summary>
		/// Name of worker who run the test.
		/// </summary>
		public string Worker { get; set; }
		// TODO define and use enum
		public string Kind { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public string ClassName { get; set; }
		public string MethodName { get; set; }		
		// TODO define and use enum
		public string Result { get; set; } // Passed, Inconclusive, Skipped, Failed
		public int Asserts { get; set; }
		
		public string Output { get; set; }
		public string Error { get; set; }
		public string StackTrace { get; set; }
		public string IgnoreReason { get; set; }

		public double Duration { get; set; }
	}
}