using System;
using System.Configuration;
using System.Linq;
using System.Net;
using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using PTR.Agent.Runtime;
using PTR.Core.Extensions;
using PTR.Core.Messages;

namespace PTR.Agent
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var cmd = new Shell.Command(args);

			//akka.tcp://TestSystem@localhost:8090/user/TestCoordinator
			HoconRoot hocon = Parser.Parse(((AkkaConfigurationSection)ConfigurationManager.GetSection("akka")).Hocon.Content);

			var ip = cmd.Options.Get("ip", GetIpAddress());
			var port = cmd.Options.Get("port", hocon.Get("akka.remote.helios.tcp.port"));
			var masterIp = cmd.Options.Get("masterIp", hocon.Get("akka.remote.helios.tcp.master-path"));
			var masterPort = cmd.Options.Get("masterPort",  hocon.Get("akka.remote.helios.tcp.master-port"));

			hocon.Set("akka.remote.helios.tcp.hostname", ip);
			hocon.Set("akka.remote.helios.tcp.port", port);

			var configurationData = ConfigurationFactory.ParseString(hocon.Value.ToString());

			using (var system = ActorSystem.Create("RemoteSystem", configurationData)) 
			{
				var masterAddress = string.Format("akka.tcp://{0}@{1}:{2}/user/{3}",
					"TestSystem",
					masterIp,
					masterPort,
					"TestCoordinator");

				var selfAddress = string.Format("akka.tcp://{0}@{1}:{2}/",
					"RemoteSystem",
					ip,
					port);

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
