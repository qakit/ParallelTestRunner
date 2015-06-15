using System;
using NUnit.Core;

namespace PTR.Core.Reporters
{
	public class ConsoleReporter : MarshalByRefObject, IReporter
	{
		public void RunStarted(string name, int testCount) { }

		public void RunFinished(TestResult result) { }

		public void RunFinished(Exception exception) { }

		public void TestStarted(TestName testName)
		{
			Console.WriteLine("#################### TEST {0} STARTED #####################", testName.Name);
		}

		public void TestFinished(TestResult result)
		{
//			Console.WriteLine("Test {0} finished success = {1}.", result.Name, result.IsSuccess);
		}

		public void SuiteStarted(TestName testName)
		{
//			Console.WriteLine("Suite {0} started", testName.Name);
		}

		public void SuiteFinished(TestResult result)
		{
			Console.WriteLine("Suite {0} finished", result.Name);
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