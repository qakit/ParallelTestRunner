using Akka.Actor;
using PTR.Core.Actors;
using PTR.Server.Runtime;

namespace PTR.Server
{
	internal static partial class Program
	{
		private const string PathToTestsDll = @"D:\C#Projects\akka-nunit\PTR.Server\bin\Debug\tests.dll";
		private static IActorRef Manager { get; set; }

		public static void Main(string[] args)
		{
			//TODO Start system using params 
			//parse params if any
			//if not start Sell.Run asap
			var testSystem = ActorSystem.Create("TestSystem");
			Manager = testSystem.ActorOf(Props.Create(() => new TestCoordinator()), "TestCoordinator");
			
			if(args.Length == 0)
				Shell.Run(Exec);
			else
			{
				//TODO parse args and start actors if necessary;
			}
			//			Manager.Tell(new Messages.RunTests(PathToTestsDll, new string[] { }, new string[] { }));
			//			Console.ReadLine();
		}
	}
}
