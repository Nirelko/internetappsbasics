using System;
using System.Collections.Generic;
using System.Linq;

namespace Reviews.Extensions
{
    public static class LinqExtensions
    {
        public static T Pick<T>(this IEnumerable<T> list)
        {
            return !list.Any() ? default(T) : list.ElementAt(new Random().Next(list.Count()));
        }
    }
}