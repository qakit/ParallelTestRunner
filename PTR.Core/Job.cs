using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace PTR.Core
{
	public enum Distrubution
	{
		Even,
		Single
	}

	public sealed class Job
	{
		public Job(string[] include, string[] exclude)
		{
			Include = FilterCategories(include);
			Exclude = FilterCategories(exclude);
		}
		
		public string[] Include { get; private set; }
		public string[] Exclude { get; private set; }
		public int LocalWorkers { get; set; }
		public Distrubution Distrubution { get; set; }
		public IActorRef Reporter { get; set; }
		public string AssemblyPath { get; set; }


		private static string[] FilterCategories(IEnumerable<string> input)
		{
			return (from s in input ?? Enumerable.Empty<string>()
					let s2 = (s ?? "").Trim()
					where !string.IsNullOrEmpty(s2)
					select s2).ToArray();
		}
	}
}
