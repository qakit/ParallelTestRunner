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
	internal static partial class MasterProgram
	{
		// TODO allow specify working dir
		private static readonly string WorkingDir = Environment.CurrentDirectory;

		private static HoconRoot Hocon { get; set; }
		private static ActorSystem Scene { get; set; }
		private static IActorRef Manager { get; set; }
		private static ProcessList Slaves { get; set; }

		// number of local workers
		private static int NumWorkers { get; set; }

		static void Main(string[] args)
		{
			// TODO specify log level from CLI args
			var opts = args.ParseOptions();
			NumWorkers = opts.Get("workers", 4);
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
			Hocon = hocon;

			using (Slaves = new ProcessList())
			{
				Start();

				// now push assemblies to be tested
				foreach (var path in input.Select(p => Path.IsPathRooted(p) ? p : Path.Combine(WorkingDir, p)))
				{
					Manager.Tell(new RunTests(path, include, exclude));
				}

				Shell.Run(Exec);

				Stop();
			}
		}

		private static void Start()
		{
			if (Scene != null) return;

			var hoconString = Hocon.Value.ToString();
			var config = ConfigurationFactory.ParseString(hoconString);

			Scene = ActorSystem.Create("TestSystem", config);
			Manager = Scene.ActorOf<Manager>("manager");

			var pid = Process.GetCurrentProcess().Id;

			// spawn local workers
			for (int i = 1; i <= NumWorkers; i++)
			{
				SpawnSlave();
			}

			Console.WriteLine("master-{0}@{1} is online", pid, Manager.Path.ToStringWithAddress());
		}

		private static void SpawnSlave()
		{
			var process = Process.Start("slave.exe");
			Slaves.Add(process);
		}

		private static void CreateLocalWorker(int i)
		{
			var pid = Process.GetCurrentProcess().Id;
			var worker = Scene.ActorOf<Worker>(string.Format("lw{0}-{1}", pid, i));
			worker.Tell(new SetMaster(Scene.ActorSelection(Manager.Path)));
		}

		private static async void Stop()
		{
			if (Scene == null) return;

			var pid = Process.GetCurrentProcess().Id;
			Console.WriteLine("master-{0}@{1} is offline", pid, Manager.Path.ToStringWithAddress());

			try
			{
				await Manager.GracefulStop(TimeSpan.FromSeconds(5));
			}
			catch (TaskCanceledException)
			{
			}

			Scene.Shutdown();
			Scene.Dispose();
			Scene = null;
		}

		private static string GetIpAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily.ToString() == "InterNetwork");
			return ip != null ? ip.ToString() : "127.0.0.1";
		}
	}
}
