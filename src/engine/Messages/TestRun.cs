using System.Collections.Generic;
using System.Linq;

namespace Akka.NUnit.Runtime.Messages
{
	public sealed class TestRun
	{
		public TestRun(string assembly, string[] include, string[] exclude)
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
}
