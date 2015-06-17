using Akka.Actor;
using PTR.Core;
using PTR.Core.Actors;
using PTR.Core.Reporters;
using PTR.Server.Runtime;

namespace PTR.Server
{
	internal static partial class Program
	{
		private static IActorRef Manager { get; set; }
		private static IActorRef TestReporter { get; set; }

		public static void Main(string[] args)
		{
			//TODO Start system using params 
			//parse params if any
			//if not start Sell.Run asap
			var testSystem = ActorSystem.Create("TestSystem");
			Manager = testSystem.ActorOf(Props.Create(() => new TestCoordinator()), "TestCoordinator");
			TestReporter = testSystem.ActorOf(Props.Create(() => new TestReporter()), "TestReporter");
			//Set console reporter by default;
			TestReporter.Tell(new SetReporter(new ConsoleReporter()));

			if(args.Length == 0)
				Shell.Run(Exec);
			else
			{
				//TODO parse args and start actors if necessary;
				Exec(new Shell.Command(args));
			}
		}
	}
}
