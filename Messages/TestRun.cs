namespace Akka.NUnit.Runtime
{
	public class TestRun
	{
		public TestRun(string assembly)
		{
			Assembly = assembly;
		}

		public string Assembly { get; private set; }
	}
}