using Akka.Actor;

namespace Akka.NUnit.Runtime.Messages
{
	public sealed class SetMaster
	{
		public SetMaster(ActorSelection master)
		{
			Master = master;
		}

		public ActorSelection Master { get; private set; }
	}
}