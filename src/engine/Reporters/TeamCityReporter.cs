using System.Globalization;
using System.IO;
using Akka.NUnit.Runtime.Messages;
using NUnit.Core;

namespace Akka.NUnit.Runtime.Reporters
{
	/// <summary>
	/// Contains methods for issuing TeamCity service messages on the Console.
	/// </summary>
	internal sealed class TeamCityReporter
	{
		private readonly TextWriter _writer;

		/// <summary>
		/// Construct a TeamCityEventHandler
		/// </summary>
		/// <param name="writer">TextWriter to which output should be directed</param>
		public TeamCityReporter(TextWriter writer)
		{
			_writer = writer;
		}

		public void Report(TestEvent e)
		{
			switch (e.Kind)
			{
				case EventKind.SuiteStarted:
					TestSuiteStarted(e.FullName);
					break;
				case EventKind.TestStarted:
					TestStarted(e.FullName);
					break;
				case EventKind.TestFinishied:
					TestFinished(e);
					break;
				case EventKind.SuiteFinished:
					TestSuiteFinished(e.FullName);
					break;
			}
		}

		private void TestStarted(string name)
		{
			_writer.WriteLine("##teamcity[testStarted name='{0}' captureStandardOutput='true']", Escape(name));
		}

		private void TestFinished(TestEvent e)
		{
			switch (e.Result)
			{
				case ResultState.Success:
					TC_TestFinished(e.FullName, e.Duration);
					break;
				case ResultState.Inconclusive:
					TC_TestIgnored(e.FullName, "Inconclusive");
					break;
				case ResultState.Skipped:
					TC_TestIgnored(e.FullName, e.Message);
					break;
				case ResultState.Failure:
				case ResultState.Error:
					TC_TestFailed(e.FullName, e.Message, e.StackTrace);
					TC_TestFinished(e.FullName, e.Duration);
					break;
			}
		}

		private void TestSuiteStarted(string name)
		{
			_writer.WriteLine("##teamcity[testSuiteStarted name='{0}']", Escape(name));
		}

		private void TestSuiteFinished(string name)
		{
			_writer.WriteLine("##teamcity[testSuiteFinished name='{0}']", Escape(name));
		}

		#region Helper Methods

		private void TC_TestFinished(string name, double duration)
		{
			_writer.WriteLine("##teamcity[testFinished name='{0}' duration='{1}']", Escape(name),
				duration.ToString("0.000", NumberFormatInfo.InvariantInfo));
		}

		private void TC_TestIgnored(string name, string reason)
		{
			_writer.WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", Escape(name), Escape(reason));
		}

		private void TC_TestFailed(string name, string message, string details)
		{
			_writer.WriteLine("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", Escape(name), Escape(message), Escape(details));
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