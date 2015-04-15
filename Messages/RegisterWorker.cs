using Akka.Actor;

namespace Akka.NUnit.Runtime.Messages
{
	public sealed class RegisterWorker
	{
		public RegisterWorker(IActorRef worker)
		{
			Worker = worker;
		}

		public IActorRef Worker { get; private set; }
	}
}