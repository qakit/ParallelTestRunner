namespace Akka.NUnit.Runtime.Messages
{
	public class Job
	{
		public Job(string assembly, string testFixture, string[] tests, string artifactsUrl)
		{
			Assembly = assembly;
			TestFixture = testFixture;
			Tests = tests;
			ArtifactsUrl = artifactsUrl;
		}

		public string Assembly { get; private set; }
		public string TestFixture { get; private set; }
		public string[] Tests { get; private set; }
		public string ArtifactsUrl { get; private set; }
	}
}
