using Akka.Actor;

namespace PTR.Core
{
	internal class RunningJob
	{
		public readonly IActorRef Worker;
		public readonly Task Task;

		public RunningJob(IActorRef worker, Task task)
		{
			Worker = worker;
			Task = task;
		}
	}
}