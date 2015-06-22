using System;
using System.Configuration;
using Akka.Actor;
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
			HoconRoot config = Parser.Parse(((AkkaConfigurationSection)ConfigurationManager.GetSection("akka")).Hocon.Content);

			var ip = cmd.Options.Get("ip", "localhost");
			var port = cmd.Options.Get("port", config.Get("akka.remote.helios.tcp.port"));
			var mIp = cmd.Options.Get("masterIp", config.Get("akka.remote.helios.tcp.master-path"));
			var mPort = cmd.Options.Get("masterPort",  config.Get("akka.remote.helios.tcp.master-port"));

			using (var system = ActorSystem.Create("RemoteSystem"))
			{
				var masterAddress = string.Format("akka.tcp://{0}@{1}:{2}/user/{3}",
					"TestSystem",
					mIp,
					mPort,
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
