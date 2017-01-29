using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ApiHooker.Communication;
using ApiHooker.Utils;

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
                //client.ShowMessageBox("Hello World!");

                var hookedMethods = client.HookFuncs(new[]
                {
                    ApiDefinitions.SetConsoleTitleA,
                    ApiDefinitions.SetConsoleWindowInfo,
                    ApiDefinitions.SetConsoleScreenBufferSize,
                    ApiDefinitions.WriteConsoleOutputA
                });

                testApp.ResumeMainThread();

                Thread.Sleep(500);

                var buffer = client.ReadBuffer();
                var callRecs = SerializationHelper.ProcessCallRecords(buffer, hookedMethods);

                client.TerminateInjectionThread();
                testApp.Process.WaitForExit();
            }

            Console.WriteLine("Before exit.");
            Console.ReadLine();
        }
    }
}
