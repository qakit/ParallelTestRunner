using System.Collections.Generic;
using System.Linq;

namespace Akka.NUnit.Runtime.Messages
{
	public sealed class RunTests
	{
		public RunTests(string assembly, string[] include, string[] exclude, string artifactsPath)
		{
			Assembly = assembly;
			Include = FilterCategories(include);
			Exclude = FilterCategories(exclude);
		    ArtifactsPath = artifactsPath;
		}

		public string Assembly { get; private set; }
		public string[] Include { get; private set; }
		public string[] Exclude { get; private set; }
        public string ArtifactsPath { get; private set; }

		private static string[] FilterCategories(IEnumerable<string> input)
		{
			return (from s in input ?? Enumerable.Empty<string>()
				let s2 = (s ?? "").Trim()
				where !string.IsNullOrEmpty(s2)
				select s2).ToArray();
		}
	}
}
