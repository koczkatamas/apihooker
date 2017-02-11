using System;
using System.IO;
using System.Text;

namespace LiveObjects.Utils.ExtensionMethods
{
    public static class BinaryWriterExtensions
    {
        public static void WriteLenPrefixed(this BinaryWriter bw, byte[] buffer)
        {
            bw.Write((UInt32) buffer.Length);
            bw.Write(buffer);
        }

        public static void WriteLenPrefixed(this BinaryWriter bw, string str)
        {
            var buffer = Encoding.ASCII.GetBytes(str);
            bw.Write((UInt32)buffer.Length);
            bw.Write(buffer);
        }
    }
}