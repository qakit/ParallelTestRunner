namespace Akka.NUnit.Runtime.Messages
{
	public sealed class TestRun
	{
		public TestRun(string assembly)
		{
			Assembly = assembly;
		}

		public string Assembly { get; private set; }
	}
}