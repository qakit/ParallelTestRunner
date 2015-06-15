using System;
using System.Linq;

namespace PTR.Core.Extensions
{
	internal static class ObjectExtensions
	{
		public static TR IfNotNull<TI, TR>(this TI input, Func<TI, TR> eval) where TI : class
		{
			return input != null ? eval(input) : default(TR);
		}

		public static bool In(this string cat, string[] cats)
		{
			return cats == null || cats.Length == 0
				   || cats.Any(s => string.Equals(s, cat, StringComparison.InvariantCultureIgnoreCase));
		}

		public static bool NotIn(this string cat, string[] cats)
		{
			return cats == null || cats.All(s => !string.Equals(s, cat, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}