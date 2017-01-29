using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
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

        public void SendPacket(PacketType type, byte[] data = null)
        {
            TcpStream.Write(BitConverter.GetBytes((uint)type));
            TcpStream.Write(BitConverter.GetBytes(data == null ? 0 : (uint)data.Length));
            if (data != null) TcpStream.Write(data);
            TcpStream.Flush();
        }

        public void ShowMessageBox(string text)
        {
            SendPacket(PacketType.MsgBox, Encoding.Default.GetBytes(text));
        }

        public void TerminateInjectionThread()
        {
            SendPacket(PacketType.Terminate);
        }

        public void Dispose()
        {
            TcpStream?.Dispose();
            ((IDisposable) TcpClient)?.Dispose();
        }
    }
}