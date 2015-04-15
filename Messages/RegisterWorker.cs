using Akka.Actor;

namespace Akka.NUnit.Runtime
{
	public class RegisterWorker
	{
		public IActorRef Worker { get; set; }

		public RegisterWorker(IActorRef worker)
		{
			Worker = worker;
		}
	}
}