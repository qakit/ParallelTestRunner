using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	internal sealed class TestFilterBuilder
	{
		public TestFilterBuilder(string[] tests = null, string[] include = null, string[] exclude = null)
		{
			Tests = new List<string>(FilterCategories(tests));
			Include = new List<string>(FilterCategories(include));
			Exclude = new List<string>(FilterCategories(exclude));
		}

		public IList<string> Tests { get; private set; }
		public IList<string> Include { get; private set; }
		public IList<string> Exclude { get; private set; }

		public TestFilter GetFilter()
		{
			var content = new[]
			{
				Tests.Count > 0 ? new XElement("tests", from t in Tests select new XElement("test", t)) : null,
				Include.Count > 0 ? new XElement("cat", string.Join(",", Include.ToArray())) : null,
				Exclude.Count > 0 ? new XElement("not", new XElement("cat", string.Join(",", Exclude.ToArray()))) : null
			}.Where(e => e != null).ToArray();

			var xml = new XElement("filter", content).ToString();
			
			return new TestFilter(xml);
		}

		private static string[] FilterCategories(IEnumerable<string> input)
		{
			return (from s in input ?? Enumerable.Empty<string>()
					let s2 = (s ?? "").Trim()
					where !string.IsNullOrEmpty(s2)
					select s2).ToArray();
		}
	}
}
