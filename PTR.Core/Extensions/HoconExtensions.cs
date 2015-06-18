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
			string[] path;
			var obj = HoconObject(root, key, out path);

			var str = Convert.ToString(value, CultureInfo.InvariantCulture);
			var prop = path[path.Length - 1];
			obj.Items[prop] = new HoconValue { Values = { new HoconLiteral { Value = str } } };
		}

		public static string Get(this HoconRoot root, string key)
		{
			string[] path;
			var obj = HoconObject(root, key, out path);

			var prop = path[path.Length - 1];
			var value = obj.Items[prop];

			return value.ToString();
		}

		private static HoconObject HoconObject(HoconRoot root, string key, out string[] path)
		{
			if (root == null) throw new ArgumentNullException("root");

			var obj = root.Value.GetObject();
			if (obj == null) throw new ArgumentException("root is not object");

			path = key.Split('.');

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
			return obj;
		}
	}
}
