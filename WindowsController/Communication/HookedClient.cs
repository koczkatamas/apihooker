using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ApiHooker.Model;
using ApiHooker.UiApi;
using ApiHooker.Utils;
using ApiHooker.Utils.ExtensionMethods;

namespace ApiHooker.Communication
{
    public class HookedClient: IDisposable
    {
        public enum PacketType : uint
        {
            MsgBox = 1,
            ReadBuffer = 2,
            Terminate = 3,
            HookFuncs = 4,
        }

        public TcpClient TcpClient { get; protected set; }
        public Stream TcpStream { get; protected set; }

        public HookedClient(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            TcpStream = tcpClient.GetStream();
        }

        public void Dispose()
        {
            TcpStream?.Dispose();
            ((IDisposable)TcpClient)?.Dispose();
        }

        public void SendPacket(PacketType type, byte[] data = null)
        {
            TcpStream.Write(BitConverter.GetBytes((uint)type));
            TcpStream.Write(BitConverter.GetBytes(data == null ? 0 : (uint)data.Length));
            if (data != null) TcpStream.Write(data);
            TcpStream.Flush();
        }

        public byte[] Call(PacketType type, byte[] inputData = null)
        {
            SendPacket(type, inputData);

            var length = BitConverter.ToUInt32(TcpStream.Read(4), 0);
            var resultData = TcpStream.Read((int)length);
            return resultData;
        }

        public void ShowMessageBox(string text)
        {
            SendPacket(PacketType.MsgBox, Encoding.Default.GetBytes(text));
        }

        public void TerminateInjectionThread()
        {
            SendPacket(PacketType.Terminate);
        }

        public byte[] ReadBuffer()
        {
            return Call(PacketType.ReadBuffer);
        }

        public class HookedMethods
        {
            public Dictionary<uint, HookedMethod> MethodIds { get; set; } = new Dictionary<uint, HookedMethod>();
        }

        public HookedMethods HookFuncs(HookedMethod[] methods)
        {
            if (methods.Length == 0)
                return new HookedMethods();

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(methods.Length);
            foreach (var m in methods)
            {
                bw.WriteLenPrefixed(m.ApiMethod.DllName);
                bw.WriteLenPrefixed(m.ApiMethod.MethodName);
                bw.Write((byte)(m.SaveCallback ? 1 : 0));
                bw.WriteLenPrefixed(SerializationHelper.SerializeFieldDescriptors(m.ApiMethod.Arguments.ToArray()));
            }

            var response = Call(PacketType.HookFuncs, ms.ToArray());
            var funcIds = Enumerable.Range(0, methods.Length).Select(i => BitConverter.ToInt32(response, i * 4)).ToArray();
            var result = new HookedMethods { MethodIds = methods.Select((m, i) => new { id = funcIds[i], m }).Where(x => x.id != -1).ToDictionary(x => (uint)x.id, x => x.m) };
            return result;
        }
    }
}