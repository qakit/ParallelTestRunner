using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.NUnit.Runtime;
using Akka.NUnit.Runtime.Messages;

namespace Akka.NUnit
{
	partial class MasterProgram
	{
		private static bool Exec(Shell.Command cmd)
		{
			switch (cmd.Name.ToLowerInvariant())
			{
				case "help":
					Console.WriteLine("q[uit] | exit - stop master");
					Console.WriteLine("run [--include=cat1,cat2,...] [--exclude=cat1,cat2,...] <assemblies> - new test run");
					Console.WriteLine("sim - run simulation");
					break;

				case "q":
				case "quit":
				case "exit":
					return false;

				case "start":
					// TODO num of local workers
					Start();
					break;

				case "stop":
					Stop();
					break;

				case "run":
					Start();
					var include = cmd.Options.Get("include", "").Split(',', ';');
					var exclude = cmd.Options.Get("exclude", "").Split(',', ';');
					foreach (var path in cmd.Input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(WorkingDir, p)))
					{
						Manager.Tell(new TestRun(path, include, exclude));
					}
					break;

				case "slave":
					SpawnSlave();
					break;

				case "sim":
					Simulate(Manager);
					break;
			}

			return true;
		}

		private static void Simulate(IActorRef manager)
		{
			var random = new Random();

			Task.Run(async () =>
			{
				for (var i = 0; i < 1; i++)
				{
					await Task.Delay(TimeSpan.FromMilliseconds(random.Next(100, 200)));

					manager.Tell(new TestRun(Path.Combine(WorkingDir, "tests.dll"), null, null));
				}
			});
		}
	}
}
