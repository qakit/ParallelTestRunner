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

	public class RequestTask
	{
		public static readonly RequestTask Instance = new RequestTask();
	}

	public class Busy
	{
		public static readonly Busy Instance = new Busy();
	}

	public class TaskCompleted
	{
		public static readonly TaskCompleted Instance = new TaskCompleted();
	}

	public class Bye
	{
		public static readonly Bye Instance = new Bye();
	}
}
