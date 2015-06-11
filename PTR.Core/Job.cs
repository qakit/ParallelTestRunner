namespace PTR.Core
{
	public sealed class Job
	{
		public Job(string assembly, string testFixture)
		{
			Assembly = assembly;
			TestFixture = testFixture;
		}

		public string Assembly { get; private set; }
		public string TestFixture { get; private set; }
	}
}
