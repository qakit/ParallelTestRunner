using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration.Hocon;
using Akka.NUnit.Runtime;
using Akka.NUnit.Runtime.Messages;

namespace Akka.NUnit
{
	internal static class SlaveProgram
	{
		private static readonly string WorkingDir = Environment.CurrentDirectory;

		static void Main(string[] args)
		{
			// forcing to load addins from app dir since nunit loads addins from user AppData folder
			Environment.SetEnvironmentVariable("MONO_ADDINS_REGISTRY", WorkingDir);

			var opts = args.ParseOptions();
			var numWorkers = opts.Get("workers", 1);
			var masterUrl = opts.Get("master", "");
			var port = opts.Get("port", 8091);

			var pid = Process.GetCurrentProcess().Id;
			var ip = GetIpAddress();

			Console.WriteLine("slave@{0} pid:{1} is up", ip, pid);

			// allow to specify only last part of ip address, e.g. 12
			int n;
			if (!string.IsNullOrEmpty(masterUrl) && int.TryParse(masterUrl, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
			{
				var ns = n.ToString(CultureInfo.InvariantCulture);
				ip = string.Join(".", ip.Split('.').Take(ip.Length - 1).Concat(new[] { ns }).ToArray());
			}
			
			if (string.IsNullOrEmpty(masterUrl))
			{
				masterUrl = string.Format("akka.tcp://TestSystem@{0}:{1}/user/manager", ip, port);
			}

			var config = ((AkkaConfigurationSection) ConfigurationManager.GetSection("akka")).AkkaConfig;

			using (var system = ActorSystem.Create("TestSystem", config))
			{
				// TODO probing of multiple masters
				var master = system.ActorSelection(masterUrl);

				var workers = new List<IActorRef>();
				
				for (int i = 1; i <= numWorkers; i++)
				{
					var worker = system.ActorOf<Worker>(string.Format("w{0}-{1}", pid, i));
					workers.Add(worker);
					worker.Tell(new SetMaster(master));
				}

				Console.WriteLine("Press any key to exit...");
				Console.ReadLine();

				StopWorkers(workers);
				system.Shutdown();
			}
		}

		private static async void StopWorkers(IEnumerable<IActorRef> workers)
		{
			foreach (var worker in workers)
			{
				try
				{
					await worker.GracefulStop(TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
				}				
			}
		}

		private static string GetIpAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily.ToString() == "InterNetwork");
			return ip != null ? ip.ToString() : "127.0.0.1";
		}
	}
}
