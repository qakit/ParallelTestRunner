﻿using System.Collections.Generic;
using System.Linq;

namespace PTR.Core.Extensions
{
	static class LinqExtensions
	{
		public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
		{
			int i = 0;
			var splits = from item in list
						 group item by i++ % parts into part
						 select part.AsEnumerable();
			return splits;
		}
	}
}
