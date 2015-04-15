namespace Akka.NUnit.Runtime
{
	public class Job
	{
		public Job(string assembly, string fixture, string artifactsUrl)
		{
			Assembly = assembly;
			Fixture = fixture;
			ArtifactsUrl = artifactsUrl;
		}

		public string Assembly { get; private set; }
		public string Fixture { get; private set; }
		public string ArtifactsUrl { get; private set; }
	}
}
