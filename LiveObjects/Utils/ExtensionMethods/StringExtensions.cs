using System;

namespace LiveObjects.Utils.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string LcFirst(this string str) => String.IsNullOrEmpty(str) ? str : char.ToLower(str[0]) + str.Substring(1);
        public static string[] Split(this string str, string separator) => str.Split(new [] { separator }, StringSplitOptions.None);
        public static string[] Split(this string str, string separator, int count) => str.Split(new[] { separator }, count, StringSplitOptions.None);
    }
}