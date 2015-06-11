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
					Console.WriteLine("q[uit] | exit - stop master");
					Console.WriteLine("run [--include=cat1,cat2,...] [--exclude=cat1,cat2,...] --artifactsPath=<artifacts> <assemblies> - new test run");
					Console.WriteLine("sim - run simulation");
					break;

				case "q":
				case "quit":
				case "exit":
					return false;
//				case "start":
//					// TODO num of local workers
//					Start();
//					break;
//
//				case "stop":
//					Stop();
//					break;

				case "run":
					var include = cmd.Options.Get("include", "").Split(',', ';');
					var exclude = cmd.Options.Get("exclude", "").Split(',', ';');

//					foreach (var path in cmd.Input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(Environment.CurrentDirectory, p)))
//					{
//						var artifactsPath = cmd.Options.Get("artifactsPath", new FileInfo(path).Directory.FullName);
						Manager.Tell(new RunTests(PathToTestsDll, include, exclude));
//					}
					break;

//				case "slave":
//				case "spawn":
//					SpawnSlave();
//					break;
//
//				case "sim":
//					Simulate(Manager);
//					break;
//
//				case "tc":
//				case "teamcity":
//					EnsureManager();
//					Manager.Tell(new SetReporter(ReporterKind.TeamCity));
//					break;
			}

			return true;
		}
	}
}
