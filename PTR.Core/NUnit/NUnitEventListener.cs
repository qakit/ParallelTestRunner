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
			_reporter.RunFinished(ConvertToEvent(result, EventKind.RunFinished));
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
			_reporter.TestFinished(ConvertToEvent(result, EventKind.TestFinishied));
		}

		public void SuiteStarted(TestName testName)
		{
			_reporter.SuiteStarted(testName);
		}

		public void SuiteFinished(TestResult result)
		{
			_reporter.SuiteFinished(ConvertToEvent(result, EventKind.SuiteFinished));
		}

		public void UnhandledException(Exception exception)
		{
			_reporter.UnhandledException(exception);
		}

		public void TestOutput(global::NUnit.Core.TestOutput testOutput)
		{
			_reporter.TestOutput(testOutput);
		}

		private TestEvent ConvertToEvent(TestResult result, EventKind kind)
		{
			return new TestEvent
			{
				Kind = kind,
				Result = result.ResultState,
				Message = result.Message,
				StackTrace = result.StackTrace,
				Duration = result.Time,
				FullName = result.Test.TestName.FullName
			};
		}

		private TestEvent ConvertToEvent(Exception error, EventKind kind)
		{
			return new TestEvent
			{
				Kind = kind,
				Error = error
			};
		}
	}
}
