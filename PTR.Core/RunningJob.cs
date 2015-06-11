using Akka.Actor;

namespace PTR.Core
{
	internal class RunningJob
	{
		public readonly IActorRef Worker;
		public readonly Job Job;

		public RunningJob(IActorRef worker, Job job)
		{
			Worker = worker;
			Job = job;
		}
	}
}