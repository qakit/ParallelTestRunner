using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	internal sealed class TestFilterBuilder
	{
		public TestFilterBuilder()
		{
			Tests = new List<string>();
			Include = new List<string>();
			Exclude = new List<string>();
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
	}
}
