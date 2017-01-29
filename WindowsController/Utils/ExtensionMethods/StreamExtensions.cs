using System;
using System.IO;
using System.Threading.Tasks;

namespace ApiHooker.Utils.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string LcFirst(this string str) => String.IsNullOrEmpty(str) ? str : char.ToLower(str[0]) + str.Substring(1);
    }

    public static class StreamExtensions
    {
        public static async Task<byte[]> ReadAsync(this Stream stream, int length)
        {
            var buffer = new byte[length];
            await stream.ReadAsync(buffer, 0, length);
            return buffer;
        }

        public static byte[] Read(this Stream stream, int length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        public static async Task WriteAsync(this Stream stream, byte[] buffer)
        {
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}