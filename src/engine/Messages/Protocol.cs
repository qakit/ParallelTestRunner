namespace Akka.NUnit.Runtime.Messages
{
	// COMMON MESSAGES

	public sealed class Greet {}

	public sealed class Bye
	{
		public Bye(string reason = null)
		{
			Reason = reason;
		}

		public string Reason { get; private set; }
	}

	// MANAGER MESSAGES

	public sealed class JobIsReady { }
	public sealed class NoJob { }

	// WORKER MESSAGES

	public sealed class RequestJob { }
	public sealed class JobCompleted {}
}