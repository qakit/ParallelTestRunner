using System.Collections.Generic;
using System.Linq;

namespace PTR.Core
{
	public sealed class RunTests
	{
		public RunTests(string assembly, string[] include, string[] exclude)
		{
			Assembly = assembly;
			Include = FilterCategories(include);
			Exclude = FilterCategories(exclude);
		}

		public string Assembly { get; private set; }
		public string[] Include { get; private set; }
		public string[] Exclude { get; private set; }

		private static string[] FilterCategories(IEnumerable<string> input)
		{
			return (from s in input ?? Enumerable.Empty<string>()
				let s2 = (s ?? "").Trim()
				where !string.IsNullOrEmpty(s2)
				select s2).ToArray();
		}
	}

	public class RegisterTestActor
	{
		public string TestActorPath { get; private set; }

		public RegisterTestActor(string testActorPath)
		{
			TestActorPath = testActorPath;
		}
	}

	public class JobIsReady
	{
		public static readonly JobIsReady Instance = new JobIsReady();
	}

	public class Busy
	{
		public static readonly Busy Instance = new Busy();
	}

	public class RequestJob
	{
		public static readonly RequestJob Instance = new RequestJob();
	}

	public class NoJob
	{
		public static readonly NoJob Instance = new NoJob();

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