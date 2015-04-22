namespace Akka.NUnit.Runtime.Messages
{
	// COMMON MESSAGES

	public sealed class Greet
	{
		public static readonly Greet Instance = new Greet();
	}

	public sealed class Bye
	{
		public static readonly Bye Shutdown = new Bye("shutdown");

		public Bye(string reason = null)
		{
			Reason = reason;
		}

		public string Reason { get; private set; }
	}

	public sealed class Busy
	{
		public static readonly Busy Instance = new Busy();
	}

	// MANAGER MESSAGES

	public sealed class JobIsReady
	{
		public static readonly JobIsReady Instance = new JobIsReady();
	}

	public sealed class NoJob
	{
		public static readonly NoJob Instance = new NoJob();
	}

	// WORKER MESSAGES

	public sealed class RequestJob
	{
		public static readonly RequestJob Instance = new RequestJob();
	}

	public sealed class JobCompleted
	{
		// TODO could have a job id
	}
}
