using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NUnit.Core;
using NUnit.Util;

namespace Akka.NUnit.Runtime.Reporters
{

	[Serializable]
	class EventCollector : EventListener
	{
		private int testRunCount;
		private int testIgnoreCount;
		private int failureCount;
		private int level;

		private ConsoleWriter writer;

		StringCollection messages = new StringCollection();

		private bool debugger = false;
		private string currentTestName;

		public EventCollector(ConsoleWriter writer)
		{
			debugger = Debugger.IsAttached;
			level = 0;
			this.writer = writer;
			this.currentTestName = string.Empty;
		}

		public void RunStarted(Test[] tests)
		{
			var x = 0;
		}

		public void RunStarted(string a, int b)
		{
			var x = 0;
		}

		public void RunFinished(TestResult[] results)
		{
			var x = 0;
		}

		public void RunFinished(Exception exception)
		{
		}

		public void RunFinished(TestResult result)
		{
		}

		public void TestFinished(TestResult testResult)
		{
			if (testResult.Executed)
			{
				testRunCount++;

				if (testResult.IsFailure)
				{
					failureCount++;
					Console.Write("F");
					if (debugger)
					{
						messages.Add(string.Format("{0}) {1} :", failureCount, testResult.Test.TestName));
						messages.Add(testResult.Message.Trim(Environment.NewLine.ToCharArray()));

						string stackTrace = StackTraceFilter.Filter(testResult.StackTrace);
						string[] trace = stackTrace.Split(System.Environment.NewLine.ToCharArray());
						foreach (string s in trace)
						{
							if (s != string.Empty)
							{
								string link = Regex.Replace(s.Trim(), @".* in (.*):line (.*)", "$1($2)");
								messages.Add(string.Format("at\n{0}", link));
							}
						}
					}
				}
			}
			else
			{
				testIgnoreCount++;
				Console.Write("N");
			}


			currentTestName = string.Empty;
		}

		public void TestStarted(TestMethod testCase)
		{
			currentTestName = testCase.TestName.FullName;
		}

		public void TestStarted(TestName testName)
		{
			currentTestName = testName.FullName;
		}


		public void SuiteStarted(TestName name)
		{
			if (debugger && level++ == 0)
			{
				testRunCount = 0;
				testIgnoreCount = 0;
				failureCount = 0;
				Console.WriteLine("################################ UNIT TESTS ################################");
				Console.WriteLine("Running tests in '" + name + "'...");
			}
		}

		public void SuiteFinished(TestResult suiteResult)
		{
			if (debugger && --level == 0)
			{
				Console.WriteLine("############################################################################");

				if (messages.Count == 0)
				{
					Console.WriteLine("##############                 S U C C E S S               #################");
				}
				else
				{
					Console.WriteLine("##############                F A I L U R E S              #################");

					foreach (string s in messages)
					{
						Trace.WriteLine(s);
					}
				}

				writer.WriteLine("############################################################################");
				writer.WriteLine("Executed tests : " + testRunCount);
				writer.WriteLine("Ignored tests  : " + testIgnoreCount);
				writer.WriteLine("Failed tests   : " + failureCount);
				writer.WriteLine("Total time     : " + suiteResult.Time + " seconds");
				writer.WriteLine("############################################################################");
			}
		}

		public void UnhandledException(Exception exception)
		{
			string msg = string.Format("##### Unhandled Exception while running {0}", currentTestName);

			// If we do labels, we already have a newline
			//if ( !options.labels ) writer.WriteLine();
			writer.WriteLine(msg);
			writer.WriteLine(exception.ToString());

			if (debugger)
			{
				Console.WriteLine(msg);
				Console.WriteLine(exception.ToString());
			}
		}

		public void TestOutput(TestOutput output)
		{
		}
	}

//	/// <summary>
//	/// Processes NUnit runner events.
//	/// </summary>
//	internal sealed class EventListener : ITestEventListener
//	{
//		private readonly string _workerName;
//		private readonly Action<TestEvent> _handler;
//
//		public EventListener(string workerName, Action<TestEvent> handler)
//		{
//			if (handler == null) throw new ArgumentNullException("handler");
//
//			_workerName = workerName;
//			_handler = handler;
//		}
//
//		public void OnTestEvent(string report)
//		{
//			var e = ParseEvent(XElement.Parse(report));
//			_handler(e);
//		}
//
//		private TestEvent ParseEvent(XElement e)
//		{
//			double duration;
//			double.TryParse(e.GetAttribute("duration", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out duration);
//
//			var output = e.Element("output").IfNotNull(x => x.Value);
//			var reason = e.XPathSelectElement("reason/message").IfNotNull(x => x.Value);
//			var error = e.XPathSelectElement("failure/message").IfNotNull(x => x.Value);
//			var stackTrace = e.XPathSelectElement("failure/stack-trace").IfNotNull(x => x.Value);
//
//			var name = e.GetAttribute("name", "");
//
//			return new TestEvent
//			{
//				Worker = _workerName,
//				Kind = e.Name.LocalName,
//				Id = e.GetAttribute("id", 0),
//				Name = name,
//				FullName = e.GetAttribute("fullname", (string) null) ?? name,
//				ClassName = e.GetAttribute("classname", ""),
//				MethodName = e.GetAttribute("methodname", ""),
//				Result = e.GetAttribute("result", "").ToUpperInvariant(),
//				Asserts = e.GetAttribute("asserts", 0),
//				Duration = duration,
//
//				Output = output ?? string.Empty,
//				Error = error ?? string.Empty,
//				StackTrace = stackTrace ?? string.Empty,
//				IgnoreReason = reason
//			};
//		}
//	}
}