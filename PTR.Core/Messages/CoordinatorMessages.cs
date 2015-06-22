using PTR.Core.Reporters;

namespace PTR.Core.Messages
{
	public class GetStatus
	{
		public static GetStatus Instance = new GetStatus();
	}

	public enum Status
	{
		Completed,
		Busy
	}

	public class TaskIsReady
	{
		public static readonly TaskIsReady Instance = new TaskIsReady();
	}

	public class NoTask
	{
		public static readonly NoTask Instance = new NoTask();
	}

	public class Greet
	{
		public string Message { get; private set; }

		public Greet(string msg)
		{
			Message = msg;
		}
	}

	public class Init
	{
		public int LocalWorkers { get; set; }
		public RunningMode RunningMode { get; set; }
	}

	/// <summary>
	/// Reporter message.
	/// </summary>
	public class SetReporter
	{
		public IReporter Reporter { get; private set; }

		public SetReporter(IReporter reporter)
		{
			Reporter = reporter;
		}
	}
}
