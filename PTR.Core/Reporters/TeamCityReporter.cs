using System;
using System.Globalization;
using NUnit.Core;
using PTR.Core.NUnit;

namespace PTR.Core.Reporters
{
	public class TeamCityReporter : MarshalByRefObject, IReporter
	{
		public void RunStarted(string name, int testCount) { }

		public void RunFinished(TestEvent result) { }

		public void RunFinished(Exception exception) { }

		public void TestStarted(TestName testName)
		{
			Console.WriteLine("##teamcity[testStarted name='{0}' captureStandardOutput='true']", Escape(testName.FullName));
		}

		public void TestFinished(TestEvent result)
		{
			var testName = result.FullName;
			switch (result.Result)
			{
				case ResultState.Success:
					TC_TestFinished(testName, result.Duration);
					break;
				case ResultState.Inconclusive:
					TC_TestIgnored(testName, "Inconclusive");
					break;
				case ResultState.Skipped:
				case ResultState.Ignored:
					TC_TestIgnored(testName, result.Message);
					break;
				case ResultState.Failure:
				case ResultState.Error:
					TC_TestFailed(testName, result.Message, result.StackTrace);
					TC_TestFinished(testName, result.Duration);
					break;
			}
		}

		public void SuiteStarted(TestName testName)
		{
//			Console.WriteLine("##teamcity[testSuiteStarted name='{0}']", Escape(testName.FullName));
		}

		public void SuiteFinished(TestEvent result)
		{
//			Console.WriteLine("##teamcity[testSuiteFinished name='{0}']", Escape(result.FullName));
		}

		public void UnhandledException(Exception exception) { }

		public void TestOutput(global::NUnit.Core.TestOutput testOutput) { }

		#region Helpers

		private void TC_TestFinished(string name, double duration)
		{
			Console.WriteLine("##teamcity[testFinished name='{0}' duration='{1}']", Escape(name),
				duration.ToString("0.000", NumberFormatInfo.InvariantInfo));
		}

		private void TC_TestIgnored(string name, string reason)
		{
			Console.WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", Escape(name), Escape(reason));
		}

		private void TC_TestFailed(string name, string message, string details)
		{
			Console.WriteLine("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", Escape(name), Escape(message), Escape(details));
		}

		private static string Escape(string input)
		{
			return input != null
				? input.Replace("|", "||")
					.Replace("'", "|'")
					.Replace("\n", "|n")
					.Replace("\r", "|r")
					.Replace(char.ConvertFromUtf32(int.Parse("0086", NumberStyles.HexNumber)), "|x")
					.Replace(char.ConvertFromUtf32(int.Parse("2028", NumberStyles.HexNumber)), "|l")
					.Replace(char.ConvertFromUtf32(int.Parse("2029", NumberStyles.HexNumber)), "|p")
					.Replace("[", "|[")
					.Replace("]", "|]")
				: null;
		}

		#endregion
	}
}
