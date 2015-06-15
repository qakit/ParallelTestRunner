using System;
using System.IO;
using System.Linq;
using Akka.Actor;
using PTR.Core;
using PTR.Core.Extensions;
using PTR.Core.Reporters;
using PTR.Server.Runtime;

namespace PTR.Server
{
	partial class Program
	{
		private static bool Exec(Shell.Command cmd)
		{
			switch (cmd.Name)
			{
				case "help":
					PrintHelp();
					break;
				case "q":
				case "quit":
				case "exit":
					return false;
				case "run":
					var include = cmd.Options.Get("include", "").Split(',', ';');
					var exclude = cmd.Options.Get("exclude", "").Split(',', ';');
					IReporter reporter = cmd.Options.Get("teamcity", false) ? (IReporter) new TeamCityReporter() : new ConsoleReporter();
					TestReporter.Tell(new SetReporter(reporter));

					var path = cmd.Input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(Environment.CurrentDirectory, p)).FirstOrDefault();
					if (string.IsNullOrEmpty(path))
					{
						PrintHelp();
						break;
					}

					Manager.Tell(new RunTests(path, include, exclude, TestReporter));
					break;
			}

			return true;
		}

		private static void PrintHelp()
		{
			//TODO make better help output;
			Console.WriteLine("run - Run tests in specified assembly. Sample usage: run tests.dll");
			Console.WriteLine("--include - Include specified category in test run. If specified other categories will be omitted. Sample usage: --include=TestCategory");
			Console.WriteLine("--exclude - Exclude specified category from test run. If specified all categories will be runned except this one. Sample usage: --exclude=TestCategory");
			Console.WriteLine("--teamcity - Enable reporting in teamcity style. Default is console reporting. Sample usage: --teamcity");
			Console.WriteLine("q[uit] | exit - Stops PTR server;");
		}
	}
}
