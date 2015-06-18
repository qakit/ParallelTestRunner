using System;
using Newtonsoft.Json;
using NUnit.Core;
using PTR.Core.NUnit;

namespace PTR.Core.Messages
{
	public class RunStarted
	{
		public string Name { get; private set; }
		public int TestCount { get; private set; }

		public RunStarted(string name, int testCount)
		{
			Name = name;
			TestCount = testCount;
		}
	}

	public class RunFinished
	{
		public Exception Exception { get; private set; }
		public TestEvent Result { get; private set; }

		[JsonConstructor]
		public RunFinished(TestEvent result)
		{
			Result = result;
		}

		public RunFinished(Exception exception)
		{
			Exception = exception;
		}
	}

	public class TestStarted
	{
		public TestName TestName { get; private set; }

		public TestStarted(TestName testName)
		{
			TestName = testName;
		}
	}

	public class TestFinished
	{
		public TestEvent Result { get; private set; }

		public TestFinished(TestEvent result)
		{
			Result = result;
		}
	}

	public class SuiteStarted
	{
		public TestName TestName { get; private set; }

		public SuiteStarted(TestName testName)
		{
			TestName = testName;
		}
	}

	public class SuiteFinished
	{
		public TestEvent Result { get; private set; }

		public SuiteFinished(TestEvent result)
		{
			Result = result;
		}
	}

	public class UnhandledException
	{
		public Exception Exception { get; private set; }

		public UnhandledException(Exception exception)
		{
			Exception = exception;
		}
	}

	public class TestOutput
	{
		public global::NUnit.Core.TestOutput Output { get; private set; }

		public TestOutput(global::NUnit.Core.TestOutput testOutput)
		{
			Output = testOutput;
		}
	}
}
