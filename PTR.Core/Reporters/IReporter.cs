using System;
using NUnit.Core;

namespace PTR.Core.Reporters
{
	public interface IReporter
	{
		void RunStarted(string name, int testCount);
		void RunFinished(TestResult result);
		void RunFinished(Exception exception);
		void TestStarted(TestName testName);
		void TestFinished(TestResult result);
		void SuiteStarted(TestName testName);
		void SuiteFinished(TestResult result);
		void UnhandledException(Exception exception);
		void TestOutput(TestOutput testOutput);
	}
}
