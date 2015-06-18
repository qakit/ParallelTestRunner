namespace PTR.Core.Messages
{
	public class RegisterTestActor
	{
		public string TestActorPath { get; private set; }

		public RegisterTestActor(string testActorPath)
		{
			TestActorPath = testActorPath;
		}
	}

	public class RequestJob
	{
		public static readonly RequestJob Instance = new RequestJob();
	}

	public class Busy
	{
		public static readonly Busy Instance = new Busy();
	}

	public class JobCompleted
	{
		public static readonly JobCompleted Instance = new JobCompleted();
	}

	public class Bye
	{
		public static readonly Bye Instance = new Bye();
	}
}
