using System;
using System.Linq;
using System.Reflection;

namespace PTR.Core.Extensions
{
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