using Akka.Actor;

namespace PTR.Core
{
	internal class RunningTask
	{
		public readonly IActorRef Worker;
		public readonly Task Task;

		public RunningTask(IActorRef worker, Task task)
		{
			Worker = worker;
			Task = task;
		}
	}
}