using System;
using System.IO;
using System.Linq;
using Akka.Actor;
using PTR.Core;
using PTR.Core.Extensions;
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

					var path = cmd.Input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(Environment.CurrentDirectory, p)).FirstOrDefault();
					if (string.IsNullOrEmpty(path))
					{
						PrintHelp();
						break;
					}
					Manager.Tell(new RunTests(path, include, exclude));
					break;
//				case "tc":
//				case "teamcity":
//					EnsureManager();
//					Manager.Tell(new SetReporter(ReporterKind.TeamCity));
//					break;
			}

			return true;
		}

		private static void PrintHelp()
		{
			//TODO make better help output;
			Console.WriteLine("run - Run tests in specified assembly");
			Console.WriteLine("--include - Include specified category in test run. If specified other categories will be omitted. Sample usage: --include=TestCategory");
			Console.WriteLine("--exclude - Exclude specified category from test run. If specified all categories will be runned except this one. Sample usage: --exclude=TestCategory");
			Console.WriteLine("q[uit] | exit - Stops PTR server;");
		}
	}
}
