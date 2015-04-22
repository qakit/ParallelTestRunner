using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Core;
using NUnit.Util;

namespace Akka.NUnit.Runtime.Reporters
{

	[Serializable]
	public class NUnitEventListener : EventListener
	{
		private int testRunCount;
		private int testIgnoreCount;
		private int failureCount;
		private int level;

		private TextWriter outWriter;
		private TextWriter errorWriter;

		StringCollection messages;

		private bool progress = false;
		private string currentTestName;

		private readonly ArrayList unhandledExceptions = new ArrayList();

		public NUnitEventListener(TextWriter outWriter, TextWriter errorWriter)
		{
			level = 0;
			this.outWriter = outWriter;
			this.errorWriter = errorWriter;
			this.currentTestName = string.Empty;

			AppDomain.CurrentDomain.UnhandledException +=
				OnUnhandledException;
		}

		public bool HasExceptions
		{
			get { return unhandledExceptions.Count > 0; }
		}

		public void WriteExceptions()
		{
			Console.WriteLine();
			Console.WriteLine("Unhandled exceptions:");
			int index = 1;
			foreach (string msg in unhandledExceptions)
				Console.WriteLine("{0}) {1}", index++, msg);
		}

		public void RunStarted(string name, int testCount)
		{
		}

		public void RunFinished(TestResult result)
		{
		}

		public void RunFinished(Exception exception)
		{
		}

		public void TestFinished(TestResult testResult)
		{
			switch (testResult.ResultState)
			{
				case ResultState.Error:
				case ResultState.Failure:
				case ResultState.Cancelled:
					testRunCount++;
					failureCount++;

					if (progress)
						Console.Write("F");

					messages.Add(string.Format("{0}) {1} :", failureCount, testResult.Test.TestName.FullName));
					messages.Add(testResult.Message.Trim(Environment.NewLine.ToCharArray()));

					string stackTrace = StackTraceFilter.Filter(testResult.StackTrace);
					if (stackTrace != null && stackTrace != string.Empty)
					{
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
					break;

				case ResultState.Inconclusive:
				case ResultState.Success:
					testRunCount++;
					break;

				case ResultState.Ignored:
				case ResultState.Skipped:
				case ResultState.NotRunnable:
					testIgnoreCount++;

					if (progress)
						Console.Write("N");
					break;
			}

			currentTestName = string.Empty;
		}

		public void TestStarted(TestName testName)
		{
			currentTestName = testName.FullName;

			if (progress)
				Console.Write(".");
		}

		public void SuiteStarted(TestName testName)
		{
			if (level++ == 0)
			{
				messages = new StringCollection();
				testRunCount = 0;
				testIgnoreCount = 0;
				failureCount = 0;
				outWriter.WriteLine("################################ UNIT TESTS ################################");
				outWriter.WriteLine("Running tests in '" + testName.FullName + "'...");
			}
		}

		public void SuiteFinished(TestResult suiteResult)
		{
			if (--level == 0)
			{
				outWriter.WriteLine("############################################################################");

				if (messages.Count == 0)
				{
					outWriter.WriteLine("##############                 S U C C E S S               #################");
				}
				else
				{
					outWriter.WriteLine("##############                F A I L U R E S              #################");

					foreach (string s in messages)
					{
						outWriter.WriteLine(s);
					}
				}

				outWriter.WriteLine("############################################################################");
				outWriter.WriteLine("Executed tests       : " + testRunCount);
				outWriter.WriteLine("Ignored tests        : " + testIgnoreCount);
				outWriter.WriteLine("Failed tests         : " + failureCount);
				outWriter.WriteLine("Unhandled exceptions : " + unhandledExceptions.Count);
				outWriter.WriteLine("Total time           : " + suiteResult.Time + " seconds");
				outWriter.WriteLine("############################################################################");
			}
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject.GetType() != typeof(System.Threading.ThreadAbortException))
			{
				this.UnhandledException((Exception)e.ExceptionObject);
			}
		}

		public void UnhandledException(Exception exception)
		{
			// If we do labels, we already have a newline
			unhandledExceptions.Add(currentTestName + " : " + exception.ToString());
			//if (!options.labels) outWriter.WriteLine();
			string msg = string.Format("##### Unhandled Exception while running {0}", currentTestName);
			//outWriter.WriteLine(msg);
			//outWriter.WriteLine(exception.ToString());

			Trace.WriteLine(msg);
			Trace.WriteLine(exception.ToString());
		}

		public void TestOutput(TestOutput output)
		{
			switch (output.Type)
			{
				case TestOutputType.Out:
					outWriter.Write(output.Text);
					break;
				case TestOutputType.Error:
					errorWriter.Write(output.Text);
					break;
			}
		}
	}
}