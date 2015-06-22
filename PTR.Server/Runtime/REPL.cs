using System;
using System.IO;
using System.Linq;
using Akka.Actor;
using PTR.Core;
using PTR.Core.Extensions;
using PTR.Core.Messages;
using PTR.Core.Reporters;
using PTR.Server.Runtime;
using Status = PTR.Core.Messages.Status;
using Task = System.Threading.Tasks.Task;

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
					int numOfLocalWorkers = cmd.Options.Get("localrun", 1);
					//get distribution value
					var distValue = cmd.Options.Get("dist", "single");
					var distribution = string.Equals(distValue, "even", StringComparison.InvariantCultureIgnoreCase)
						? Distrubution.Even
						: Distrubution.Single;

					var runModeValue = cmd.Options.Get("mode", "inprocess");
					var runMode = string.Equals(runModeValue, "separate", StringComparison.InvariantCultureIgnoreCase)
						? RunningMode.Separate
						: RunningMode.Inprocess;

					IReporter reporter = cmd.Options.Get("teamcity", false) ? (IReporter) new TeamCityReporter() : new ConsoleReporter();
					TestReporter.Tell(new SetReporter(reporter));

					var path = cmd.Input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(Environment.CurrentDirectory, p)).FirstOrDefault();
					if (string.IsNullOrEmpty(path))
					{
						PrintHelp();
						break;
					}

					Manager.Tell(new Init
					{
						LocalWorkers = numOfLocalWorkers,
						RunningMode = runMode,
					});

					var job = new Job(include, exclude)
					{
						Distrubution = distribution,
						Reporter = TestReporter,
						AssemblyPath = path
					};

					Manager.Tell(job);
					WaitTaskCompleted().Wait();
					break;
				default:
					PrintHelp();
					break;
			}

			return true;
		}

		private static async Task WaitTaskCompleted()
		{
			while (true)
			{
				await Task.Delay(TimeSpan.FromSeconds(1));

				try
				{
					var status = await Manager.Ask<Status>(GetStatus.Instance, TimeSpan.FromSeconds(1));
					if(status == Status.Completed)
						return;
				}
				catch (Exception e)
				{
					
				}
			}
		}

		private static void PrintHelp()
		{
			//TODO make better help output;
			Console.WriteLine("run - Run tests in specified assembly. Sample usage: run tests.dll");
			Console.WriteLine("--include=<category1, category2> - Include specified category in test run. If specified other categories will be omitted. Sample usage: --include=TestCategory");
			Console.WriteLine("--exclude=<category3, category4> - Exclude specified category from test run. If specified all categories will be runned except this one. Sample usage: --exclude=TestCategory");
			Console.WriteLine("--teamcity - Enable reporting in teamcity style. Default is console reporting. Sample usage: --teamcity");
			Console.WriteLine("--localrun=<numOfLocalWorkers> - Allow server use local workers to run. <numOfLocalWorkers> specified number of local workers to be runned during test's execution: Sample usage: --localrun=2");
			Console.WriteLine("--dist=<Even, Single> - set tests distribution level for actors. Even - all testfixtures will be distributed evenly, otherwise by request (one fixture to one actor)");
			Console.WriteLine("q[uit] | exit - Stops PTR server;");
		}
	}
}
