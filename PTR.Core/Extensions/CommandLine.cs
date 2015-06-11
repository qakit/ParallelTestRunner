using System;
using System.Collections.Generic;
using System.Linq;

namespace PTR.Core.Extensions
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
			return dictionary.TryGetValue(key, out value) ? (T)Convert.ChangeType(value, typeof(T)) : defval();
		}
	}
}
