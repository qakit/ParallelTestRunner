using System;
using Akka.Actor;

namespace Akka.NUnit.Runtime
{
	class Program
	{
		static void Main(string[] args)
		{
			var system = ActorSystem.Create("PR");

			var manager = system.ActorOf<Manager>("manager");

			for (int i = 1; i <= 4; i++)
			{
				system.ActorOf<Worker>("worker" + i);
			}

			manager.Tell(new TestRun("tests.dll"));

			Console.ReadLine();
		}
	}
}
