using System;

namespace PTR.Core.Extensions
{
	internal static class ObjectExtensions
	{
		public static TR IfNotNull<TI, TR>(this TI input, Func<TI, TR> eval) where TI : class
		{
			return input != null ? eval(input) : default(TR);
		}
	}
}