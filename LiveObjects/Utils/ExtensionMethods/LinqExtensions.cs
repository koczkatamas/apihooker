using System;
using System.Collections.Generic;

namespace LiveObjects.Utils.ExtensionMethods
{
    public static class LinqExtensions
    {
        public static string Join<T>(this IEnumerable<T> values, string separator)
        {
            return String.Join(separator, values);
        }
    }
}