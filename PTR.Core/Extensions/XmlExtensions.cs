using System;
using System.Xml.Linq;

namespace PTR.Core.Extensions
{
	internal static class XmlExtensions
	{
		public static T GetAttribute<T>(this XElement e, string name, T defval)
		{
			var attr = e.Attribute(name);
			return attr != null ? (T)Convert.ChangeType(attr.Value, typeof(T)) : defval;
		}
	}
}