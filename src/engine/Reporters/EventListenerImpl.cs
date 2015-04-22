using System;
using Akka.NUnit.Runtime.Messages;
using NUnit.Core;

namespace Akka.NUnit.Runtime.Reporters
{
	internal sealed class EventListenerImpl : MarshalByRefObject, EventListener
	{
		private readonly string _workerName;
		private readonly Action<TestEvent> _handler;

		public EventListenerImpl(string workerName, Action<TestEvent> handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			_workerName = workerName;
			_handler = handler;
		}

		public void RunStarted(string name, int testCount)
		{
			_handler(new TestEvent
			{
				Kind = EventKind.RunStarted,
				Worker = _workerName,
				FullName = name,
				TestCount = testCount
			});
		}

		public void RunFinished(TestResult result)
		{
			_handler(ConvertToEvent(result, EventKind.RunFinished));
		}

		public void RunFinished(Exception exception)
		{
			_handler(ConvertToEvent(exception, EventKind.RunFinished));
		}

		public void TestStarted(TestName testName)
		{
			_handler(new TestEvent
			{
				Kind = EventKind.TestStarted,
				Worker = _workerName,
				FullName = testName.FullName
			});
		}

		public void TestFinished(TestResult result)
		{
			_handler(ConvertToEvent(result, EventKind.TestFinishied));
		}

		public void SuiteStarted(TestName testName)
		{
			_handler(new TestEvent
			{
				Kind = EventKind.SuiteStarted,
				Worker = _workerName,
				FullName = testName.FullName
			});
		}

		public void SuiteFinished(TestResult result)
		{
			_handler(ConvertToEvent(result, EventKind.SuiteFinished));
		}

		public void UnhandledException(Exception exception)
		{
		}

		public void TestOutput(TestOutput testOutput)
		{
		}

		private TestEvent ConvertToEvent(TestResult result, EventKind kind)
		{
			return new TestEvent
			{
				Worker = _workerName,
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
				Worker = _workerName,
				Kind = kind,
				Error = error
			};
		}
	}
}
