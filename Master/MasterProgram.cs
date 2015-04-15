using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.NUnit.Runtime;
using Akka.NUnit.Runtime.Messages;

namespace Akka.NUnit
{
	internal static class MasterProgram
	{
		static void Main(string[] args)
		{
			var opts = args.ParseOptions();
			var numWorkers = opts.Get("workers", 0); // number of local workers
			var port = opts.Get("port", 8091);
			var input = args.Where(a => !(a.StartsWith("--") || a.StartsWith("/"))).ToArray();

			// TODO remove when prototype is completed
			if (input.Length == 0)
			{
				input = new[] {"tests.dll"};
			}

			var ip = GetIpAddress();
			var pid = Process.GetCurrentProcess().Id;
			Console.WriteLine("starting master@{0} pid:{1}", ip, pid);

			// patch akka config
			var hocon = Parser.Parse(((AkkaConfigurationSection) ConfigurationManager.GetSection("akka")).Hocon.Content);
			hocon.Set("akka.remote.helios.tcp.port", port);
			hocon.Set("akka.remote.helios.tcp.hostname", ip);
			hocon.Set("akka.remote.helios.tcp.public-hostname", ip);

			var hoconString = hocon.Value.ToString();
			var config = ConfigurationFactory.ParseString(hoconString);
			
			using (var system = ActorSystem.Create("TestSystem", config))
			{
				var manager = system.ActorOf<Manager>("manager");

				// create local workers
				for (int i = 1; i <= numWorkers; i++)
				{
					var worker = system.ActorOf<Worker>(string.Format("lw{0}-{1}", pid, i));
					worker.Tell(new SetMaster(system.ActorSelection(manager.Path)));
				}

				Console.WriteLine("master is up {0}", manager.Path.ToStringWithAddress());

				// now enqueue assemblies to be tested

				var workDir = Environment.CurrentDirectory; // TODO allow specify working dir
				foreach (var path in input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(workDir, p)))
				{
					manager.Tell(new TestRun(path));
				}

				// TODO REPL with commands, e.g. simulate command to run tests.dll N times
				// Simulate(manager);

				Console.WriteLine("Press any key to exit...");
				Console.ReadLine();
			}
		}

		private static void Simulate(IActorRef manager)
		{
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
		}

		private static string GetIpAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily.ToString() == "InterNetwork");
			return ip != null ? ip.ToString() : "127.0.0.1";
		}
	}
}
