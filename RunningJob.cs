using Akka.Actor;

namespace Akka.NUnit.Runtime
{
	public class RunningJob
	{
		public string AssemblyPath;
		public string ArtifactsUri;
		public IActorRef Worker;
		public string TestFixtureName;
	}
}
