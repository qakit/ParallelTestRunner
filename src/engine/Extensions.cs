using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Akka.NUnit.Runtime
{
	public static class CommandLine
	{
		public static IDictionary<string, string> ParseOptions(this IEnumerable<string> args)
		{
			return (from arg in args
				where arg.StartsWith("--") || arg.StartsWith("/")
				// TODO support quoting to pass values with spaces
				let p = arg.Substring(arg.StartsWith("--") ? 2 : 1).Split('=')
				where p.Length <= 2
				let key = p[0].Trim()
				let val = p.Length == 2 ? p[1].Trim() : "true"
				select new KeyValuePair<string, string>(key, val))
				.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
		}

		public static T Get<T, TValue>(this IDictionary<string, TValue> dictionary, string key, T defval)
		{
			return dictionary.Get(key, () => defval);
		}

		public static T Get<T, TValue>(this IDictionary<string, TValue> dictionary, string key, Func<T> defval)
		{
			TValue value;
			return dictionary.TryGetValue(key, out value) ? (T) Convert.ChangeType(value, typeof(T)) : defval();
		}
	}

	internal static class XmlExtensions
	{
		public static T GetAttribute<T>(this XElement e, string name, T defval)
		{
			var attr = e.Attribute(name);
			return attr != null ? (T) Convert.ChangeType(attr.Value, typeof(T)) : defval;
		}
	}

	internal static class ObjectExtensions
	{
		public static TR IfNotNull<TI, TR>(this TI input, Func<TI, TR> eval) where TI:class
		{
			return input != null ? eval(input) : default(TR);
		}
	}

	internal static class ReflectionExtensions
	{
		public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			var attrs = (T[])provider.GetCustomAttributes(typeof(T), inherit);
			return attrs.FirstOrDefault();
		}

		public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
		{
			return provider.GetAttribute<T>() != null;
		}
	}
}
