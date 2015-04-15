using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.NUnit.Runtime;
using Akka.NUnit.Runtime.Messages;

namespace Master
{
	class MasterProgram
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Master is running");
			using (var system = ActorSystem.Create("TestSystem"))
			{
				var manager = system.ActorOf<Akka.NUnit.Runtime.Manager>("manager");
				Console.WriteLine(manager.Path.ToStringWithAddress());

				//TODO get number of workers from config file (0 by default);
				for (int i = 1; i <= 0; i++)
				{
					var worker = system.ActorOf<Worker>("worker" + i);
					worker.Tell(new SetMaster(system.ActorSelection(manager.Path)));
				}

				// await any workers

				// TODO get tests dll to run from args

				var random = new Random();

				Task.Run(async () =>
				{
					for (var i = 0; i < 1; i++)
					{
						await Task.Delay(TimeSpan.FromMilliseconds(random.Next(100, 200)));

						var workDir = Environment.CurrentDirectory;
						manager.Tell(new TestRun(Path.Combine(workDir, "tests.dll")));
					}
				});

				Console.WriteLine("Press any key to exit");
				Console.ReadLine();
			}
		}
	}
}
