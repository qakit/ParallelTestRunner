using System;
using System.Collections.Generic;
using System.Globalization;
using Akka.Configuration.Hocon;

namespace PTR.Core.Extensions
{
	public static class HoconExtensions
	{
		public static void Set<T>(this HoconRoot root, string key, T value)
		{
			if (root == null) throw new ArgumentNullException("root");

			var obj = root.Value.GetObject();
			if (obj == null) throw new ArgumentException("root is not object");

			var path = key.Split('.');

			for (var i = 0; i < path.Length - 1; i++)
			{
				HoconValue val;
				if (!obj.Items.TryGetValue(path[i], out val))
				{
					throw new KeyNotFoundException("key");
				}

				obj = val.GetObject();
				if (obj == null)
				{
					throw new KeyNotFoundException("key");
				}
			}

			var str = Convert.ToString(value, CultureInfo.InvariantCulture);
			var prop = path[path.Length - 1];
			obj.Items[prop] = new HoconValue { Values = { new HoconLiteral { Value = str } } };
		}

//		public static T Get<T, TValue>(this IDictionary<string, TValue> dictionary, string key, T defval)
//		{
//			return dictionary.Get(key, () => defval);
//		}
//
//		public static T Get<T, TValue>(this IDictionary<string, TValue> dictionary, string key, Func<T> defval)
//		{
//			TValue value;
//			return dictionary.TryGetValue(key, out value) ? (T)Convert.ChangeType(value, typeof(T)) : defval();
//		}

		public static string Get(this HoconRoot root, string key)
		{
			if (root == null) throw new ArgumentNullException("root");

			var obj = root.Value.GetObject();
			if (obj == null) throw new ArgumentException("root is not object");

			var path = key.Split('.');
			for (var i = 0; i < path.Length - 1; i++)
			{
				HoconValue val;
				if (!obj.Items.TryGetValue(path[i], out val))
				{
					throw new KeyNotFoundException("key");
				}

				obj = val.GetObject();
				if (obj == null)
				{
					throw new KeyNotFoundException("key");
				}
			}

			var prop = path[path.Length - 1];
			var value = obj.Items[prop];

			return value.ToString();
		}
	}
}
