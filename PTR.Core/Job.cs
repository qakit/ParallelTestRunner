using Akka.Actor;

namespace PTR.Core
{
	public sealed class Job
	{
		public Job(string assembly, string testFixture, IActorRef reporterActor)
		{
			Assembly = assembly;
			TestFixture = testFixture;
			ReporterActor = reporterActor;
		}

		public string Assembly { get; private set; }
		public string TestFixture { get; private set; }
		public IActorRef ReporterActor { get; private set; }
	}
}
