using System.IO;
using System.Threading.Tasks;

namespace LiveObjects.Utils.ExtensionMethods
{
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