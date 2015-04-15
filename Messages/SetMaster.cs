using Akka.Actor;

namespace Akka.NUnit.Runtime
{
	public class SetMaster
	{
		public ActorSelection Master { get; set; }

		public SetMaster(ActorSelection master)
		{
			Master = master;
		}
	}
}