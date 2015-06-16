using System;
using NUnit.Core;
using PTR.Core.NUnit;

namespace PTR.Core.Reporters
{
	public class ConsoleReporter : MarshalByRefObject, IReporter
	{
		public void RunStarted(string name, int testCount) { }

		public void RunFinished(TestEvent result) { }

		public void RunFinished(Exception exception) { }

		public void TestStarted(TestName testName)
		{
			Console.WriteLine("#################### TEST {0} STARTED #####################", testName.Name);
		}

		public void TestFinished(TestEvent result)
		{
//			Console.WriteLine("Test {0} finished success = {1}.", result.Name, result.IsSuccess);
		}

		public void SuiteStarted(TestName testName)
		{
//			Console.WriteLine("Suite {0} started", testName.Name);
		}

		public void SuiteFinished(TestEvent result)
		{
			Console.WriteLine("Suite {0} finished", result.FullName);
		}

		public void UnhandledException(Exception exception)
		{
//			Console.WriteLine("Global exception occurs {0}", exception.Message);
		}

		public void TestOutput(global::NUnit.Core.TestOutput testOutput)
		{
//			Console.WriteLine("Some test output: {0}", testOutput.Text);
		}
	}
}