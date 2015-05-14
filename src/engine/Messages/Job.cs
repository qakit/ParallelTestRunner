using System.IO;

namespace Akka.NUnit.Runtime.Messages
{
	public sealed class Job
	{
		public Job(string assembly, string testFixture, string artifactsUrl)
		{
			Assembly = assembly;
			TestFixture = testFixture;
			ArtifactsUrl = new DirectoryInfo(artifactsUrl.Trim('"'));
		}

		public string Assembly { get; private set; }
		public string TestFixture { get; private set; }
		public DirectoryInfo ArtifactsUrl { get; private set; }
	}
}
