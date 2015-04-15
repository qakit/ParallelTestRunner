using Akka.Actor;

namespace Akka.NUnit.Runtime.Messages
{
	// MANAGER MESSAGES

	public sealed class JobIsReady { }
	public sealed class NoJob { }

	// WORKER MESSAGES

	public sealed class RegisterWorker { }
	public sealed class RequestJob { }

	public sealed class JobCompleted
	{
		public JobCompleted(IActorRef worker)
		{
			Worker = worker;
		}

		public IActorRef Worker { get; private set; }
	}
}