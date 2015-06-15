using System;
using NUnit.Core;
using PTR.Core.Reporters;

namespace PTR.Core.NUnit
{
	internal sealed class NUnitEventListener : MarshalByRefObject, EventListener
	{
		private readonly IReporter _reporter;

		public NUnitEventListener(IReporter reporter)
		{
			_reporter = reporter;
		}

		public void RunStarted(string name, int testCount)
		{
			_reporter.RunStarted(name, testCount);
		}

		public void RunFinished(TestResult result)
		{
			_reporter.RunFinished(result);
		}

		public void RunFinished(Exception exception)
		{
			_reporter.RunFinished(exception);
		}

		public void TestStarted(TestName testName)
		{
			_reporter.TestStarted(testName);
		}

		public void TestFinished(TestResult result)
		{
			_reporter.TestFinished(result);
		}

		public void SuiteStarted(TestName testName)
		{
			_reporter.SuiteStarted(testName);
		}

		public void SuiteFinished(TestResult result)
		{
			_reporter.SuiteFinished(result);
		}

		public void UnhandledException(Exception exception)
		{
			_reporter.UnhandledException(exception);
		}

		public void TestOutput(global::NUnit.Core.TestOutput testOutput)
		{
			_reporter.TestOutput(testOutput);
		}
	}
}
