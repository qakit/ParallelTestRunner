using System;
using System.Configuration;
using System.Linq;
using System.Net;
using Akka.Actor;
using Akka.Configuration.Hocon;
using PTR.Core;
using PTR.Core.Extensions;

namespace PTR.Agent
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//akka.tcp://TestSystem@localhost:8090/user/TestCoordinator
			HoconRoot config = Parser.Parse(((AkkaConfigurationSection)ConfigurationManager.GetSection("akka")).Hocon.Content);
			var masterIp = config.Get("akka.remote.helios.tcp.master-path");
			var masterPort = config.Get("akka.remote.helios.tcp.master-port");
			
			var selfIp = GetIpAddress();
			var selfPort = config.Get("akka.remote.helios.tcp.port");

			using (var system = ActorSystem.Create("RemoteSystem"))
			{
				var masterAddress = string.Format("akka.tcp://{0}@{1}:{2}/user/{3}",
					"TestSystem",
					masterIp,
					masterPort,
					"TestCoordinator");

				var selfAddress = string.Format("akka.tcp://{0}@{1}:{2}/",
					"RemoteSystem",
					"localhost",
					selfPort);

				var masterSelection = system.ActorSelection(masterAddress);

				bool isMasterFound;
				do
				{
					isMasterFound = IsResolveSuccess(() => masterSelection.ResolveOne(TimeSpan.FromSeconds(1)).Wait());
				} while (!isMasterFound);

				masterSelection.Tell(new RegisterTestActor(selfAddress));
				Console.ReadKey();
			}
		}

		private static string GetIpAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily.ToString() == "InterNetwork");
			return ip != null ? ip.ToString() : "127.0.0.1";
		}

		private static bool IsResolveSuccess(Action resolveAction)
		{
			try
			{
				resolveAction();
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}
	}
}
