using System;
using NUnit.Core;
using PTR.Core.NUnit;

namespace PTR.Core.Reporters
{
	public interface IReporter
	{
		void RunStarted(string name, int testCount);
		void RunFinished(TestEvent result);
		void RunFinished(Exception exception);
		void TestStarted(TestName testName);
		void TestFinished(TestEvent result);
		void SuiteStarted(TestName testName);
		void SuiteFinished(TestEvent result);
		void UnhandledException(Exception exception);
		void TestOutput(global::NUnit.Core.TestOutput testOutput);
	}
}
