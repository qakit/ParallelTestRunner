using System;
using System.IO;
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

			var workDir = Environment.CurrentDirectory;
			manager.Tell(new TestRun(Path.Combine(workDir, "tests.dll")));

			Console.ReadLine();
		}
	}
}
