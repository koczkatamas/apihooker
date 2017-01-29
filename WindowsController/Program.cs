using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ApiHooker.Communication;

namespace ApiHooker
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var pkill in Process.GetProcessesByName("TestApp"))
                pkill.Kill();

            var testApp = ProcessManager.LaunchSuspended(AppDomain.CurrentDomain.BaseDirectory + "TestApp.exe");
            testApp.Inject();

            var tcpServer = new TcpListener(IPAddress.Loopback, 1337);
            tcpServer.Start();

            using (var client = new HookedClient(tcpServer.AcceptTcpClient()))
            {
                client.ShowMessageBox("Hello World!");
                client.TerminateInjectionThread();

                testApp.ResumeMainThread();
            }

            testApp.Process.WaitForExit();
        }
    }
}
