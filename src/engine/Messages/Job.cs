namespace Akka.NUnit.Runtime.Messages
{
	public class Job
	{
		public Job(string assembly, string testFixture, string artifactsUrl)
		{
			Assembly = assembly;
			TestFixture = testFixture;
			ArtifactsUrl = artifactsUrl;
		}

		public string Assembly { get; private set; }
		public string TestFixture { get; private set; }
		public string ArtifactsUrl { get; private set; }
	}
}
