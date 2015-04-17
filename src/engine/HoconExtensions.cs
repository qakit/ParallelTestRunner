using System;
using System.Collections.Generic;
using System.Globalization;
using Akka.Configuration.Hocon;

namespace Akka.NUnit.Runtime
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
			obj.Items[prop] = new HoconValue {Values = {new HoconLiteral {Value = str}}};
		}
	}
}
