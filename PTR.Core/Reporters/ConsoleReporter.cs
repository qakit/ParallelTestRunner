using System;
using NUnit.Core;

namespace PTR.Core.Reporters
{
	public class ConsoleReporter : IReporter
	{
		public void RunStarted(string name, int testCount)
		{
//			Console.WriteLine("Run {0} started with {1} tests in it", name, testCount);
		}

		public void RunFinished(TestResult result)
		{
//			Console.WriteLine("Run {0} finished", result.FullName);
		}

		public void RunFinished(Exception exception)
		{
//			Console.WriteLine("Run finished with exception {0}", exception.Message);
		}

		public void TestStarted(TestName testName)
		{
//			Console.WriteLine("Test {0} started.", testName.Name);
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
			Console.WriteLine((string) "Suite {0} finished", (object) result.Name);
		}

		public void UnhandledException(Exception exception)
		{
//			Console.WriteLine("Global exception occurs {0}", exception.Message);
		}

		public void TestOutput(TestOutput testOutput)
		{
//			Console.WriteLine("Some test output: {0}", testOutput.Text);
		}
	}
}