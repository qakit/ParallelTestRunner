using System;
using System.Configuration;
using Akka.Actor;
using Akka.Configuration.Hocon;
using Akka.NUnit.Runtime;

namespace Slave
{
	class SlaveProgram
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Slave is up");

			using (var system = ActorSystem.Create("TestSystem"))
			{
				var config = ((AkkaConfigurationSection)ConfigurationManager.GetSection("akka")).AkkaConfig;
				var master = system.ActorSelection(config.GetString("master.url"));

				//TODO get number of workers from config or args (1 by default)
				for (int i = 1; i <= 1; i++)
				{
					var worker = system.ActorOf<Worker>("worker" + i);
					worker.Tell(new SetMaster(master));
				}

				Console.WriteLine("Press any key to exit");
				Console.ReadLine();
			}
		}
	}
}
