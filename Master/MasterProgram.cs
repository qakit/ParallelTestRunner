using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.NUnit.Runtime;
using Akka.NUnit.Runtime.Messages;

namespace Akka.NUnit
{
	internal static partial class MasterProgram
	{
		// TODO allow specify working dir
		private static readonly string WorkingDir = Environment.CurrentDirectory;

		static void Main(string[] args)
		{
			// TODO specify log level from CLI args
			var opts = args.ParseOptions();
			var numWorkers = opts.Get("workers", 0); // number of local workers
			var port = opts.Get("port", 8091);
			var include = opts.Get("include", "").Split(',', ';');
			var exclude = opts.Get("exclude", "").Split(',', ';');
			var input = args.Where(a => !(a.StartsWith("--") || a.StartsWith("/"))).ToArray();

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

				// now push assemblies to be tested
				foreach (var path in input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(WorkingDir, p)))
				{
					manager.Tell(new TestRun(path, include, exclude));
				}

				CommandLoop(manager);
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
