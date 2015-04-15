using System.Globalization;
using System.IO;
using Akka.NUnit.Runtime.Messages;

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
			switch ((e.Kind ?? string.Empty).ToLowerInvariant())
			{
				case "start-suite":
					TestSuiteStarted(e.Name);
					break;
				case "start-test":
					TestStarted(e.Name);
					break;
				case "test-case":
					TestFinished(e);
					break;
				case "test-suite":
					TestSuiteFinished(e.Name);
					break;
			}
		}

		private void TestStarted(string name)
		{
			_writer.WriteLine("##teamcity[testStarted name='{0}' captureStandardOutput='true']", Escape(name));
		}

		private void TestFinished(TestEvent e)
		{
			switch ((e.Result ?? string.Empty).ToLowerInvariant())
			{
				case "passed":
					TC_TestFinished(e.Name, e.Duration);
					break;
				case "inconclusive":
					TC_TestIgnored(e.Name, "Inconclusive");
					break;
				case "skipped":
					TC_TestIgnored(e.Name, e.IgnoreReason);
					break;
				case "failed":
					TC_TestFailed(e.Name, e.Error, e.StackTrace);
					TC_TestFinished(e.Name, e.Duration);
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