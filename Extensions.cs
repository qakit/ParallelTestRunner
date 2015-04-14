using System.Xml.Linq;

namespace Akka.NUnit.Runtime
{
	internal static class XmlExtensions
	{
		public static string GetAttribute(this XElement e, string name)
		{
			var attr = e.Attribute(name);
			return attr != null ? attr.Value : null;
		}
	}
}
