using Akka.Actor;

namespace PTR.Core
{
	public sealed class Task
	{
		public Task(string assembly, string[] testFixtures, IActorRef reporterActor)
		{
			Assembly = assembly;
			TestFixtures = testFixtures;
			ReporterActor = reporterActor;
		}

		public string Assembly { get; private set; }
		public string[] TestFixtures { get; private set; }
		public IActorRef ReporterActor { get; private set; }
	}
}
