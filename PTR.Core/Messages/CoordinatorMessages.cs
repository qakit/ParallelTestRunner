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

	public class JobIsReady
	{
		public static readonly JobIsReady Instance = new JobIsReady();
	}

	public class NoJob
	{
		public static readonly NoJob Instance = new NoJob();
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
