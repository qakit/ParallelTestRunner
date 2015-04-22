using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Core;

namespace Akka.NUnit.Runtime.Reporters
{
	internal sealed class CompositeEventListener : EventListener
	{
		private readonly List<EventListener> _listeners;

		public CompositeEventListener(IEnumerable<EventListener> listeners)
		{
			_listeners = listeners.ToList();
		}

		public void RunStarted(string name, int testCount)
		{
			_listeners.ForEach(l => l.RunStarted(name, testCount));
		}

		public void RunFinished(TestResult result)
		{
			_listeners.ForEach(l => l.RunFinished(result));
		}

		public void RunFinished(Exception exception)
		{
			_listeners.ForEach(l => l.RunFinished(exception));
		}

		public void TestStarted(TestName testName)
		{
			_listeners.ForEach(l => l.TestStarted(testName));
		}

		public void TestFinished(TestResult result)
		{
			_listeners.ForEach(l => l.TestFinished(result));
		}

		public void SuiteStarted(TestName testName)
		{
			_listeners.ForEach(l => l.SuiteStarted(testName));
		}

		public void SuiteFinished(TestResult result)
		{
			_listeners.ForEach(l => l.SuiteFinished(result));
		}

		public void UnhandledException(Exception exception)
		{
			_listeners.ForEach(l => l.UnhandledException(exception));
		}

		public void TestOutput(TestOutput testOutput)
		{
			_listeners.ForEach(l => l.TestOutput(testOutput));
		}
	}
}