using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using NUnit.Core;
using PTR.Core.NUnit;
using PTR.Core.Reporters;

namespace PTR.Core
{
	public sealed class RunTests
	{
		public RunTests(string assembly, string[] include, string[] exclude, IActorRef reporterActor)
		{
			Assembly = assembly;
			ReporterActor = reporterActor;
			Include = FilterCategories(include);
			Exclude = FilterCategories(exclude);
		}

		public string Assembly { get; private set; }
		public IActorRef ReporterActor { get; private set; }
		public string[] Include { get; private set; }
		public string[] Exclude { get; private set; }

		private static string[] FilterCategories(IEnumerable<string> input)
		{
			return (from s in input ?? Enumerable.Empty<string>()
					let s2 = (s ?? "").Trim()
					where !string.IsNullOrEmpty(s2)
					select s2).ToArray();
		}
	}

	public class RegisterTestActor
	{
		public string TestActorPath { get; private set; }

		public RegisterTestActor(string testActorPath)
		{
			TestActorPath = testActorPath;
		}
	}

	public class SetReporter
	{
		public IReporter Reporter { get; private set; }

		public SetReporter(IReporter reporter)
		{
			Reporter = reporter;
		}
	}

	public class JobIsReady
	{
		public static readonly JobIsReady Instance = new JobIsReady();
	}

	public class Busy
	{
		public static readonly Busy Instance = new Busy();
	}

	public class RequestJob
	{
		public static readonly RequestJob Instance = new RequestJob();
	}

	public class NoJob
	{
		public static readonly NoJob Instance = new NoJob();

	}

	public class JobCompleted
	{
		public static readonly JobCompleted Instance = new JobCompleted();
	}

	public class Bye
	{
		public static readonly Bye Instance = new Bye();
	}

	public class Greet
	{
		public string Message { get; private set; }

		public Greet(string message)
		{
			Message = message;
		}
	}

	#region TCMessages

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
		public TestResult Result { get; private set; }

		public RunFinished(TestResult result)
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
		public TestResult Result { get; private set; }

		public TestFinished(TestResult result)
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
		public TestResult Result { get; private set; }

		public SuiteFinished(TestResult result)
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

	#endregion
}