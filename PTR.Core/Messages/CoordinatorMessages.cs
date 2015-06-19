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
