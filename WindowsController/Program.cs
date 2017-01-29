using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiHooker.Communication;
using ApiHooker.UiApi;
using ApiHooker.UiApi.JsonRpc;
using ApiHooker.Utils;
using ApiHooker.VisualStudio;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;

namespace ApiHooker
{
    class Program
    {
        static async Task UIApiAsync(CancellationToken ct)
        {
            try
            {
                var uiApi = new UIApi();
                var jsonRpc = new JsonRpc();
                jsonRpc.PublishObject(uiApi);

                var webSocket = new WebSocketListener(new IPEndPoint(IPAddress.Loopback, 1338));
                webSocket.Standards.RegisterStandard(new WebSocketFactoryRfc6455(webSocket));
                webSocket.Start();

                Console.WriteLine("[UIApi] Started.");

                while (!ct.IsCancellationRequested)
                {
                    var client = await webSocket.AcceptWebSocketAsync(ct);
                    if (client.HttpRequest.Headers["Origin"] != "http://localhost:8000")
                        client.Close();

                    while (true)
                    {
                        var request = await client.ReadStringAsync(ct);
                        var response = await jsonRpc.ProcessMessageAsync(request);
                        await client.WriteStringAsync(response, ct);
                    }
                }

                Console.WriteLine("[UIApi] Stopped.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[UIApi] Exception: {e}");
            }
        }

        static void TestApp()
        {
            var serverPort = 1337;
            var tcpServer = new TcpListener(IPAddress.Loopback, serverPort);
            tcpServer.Start();

            var testApp = ProcessManager.LaunchSuspended(AppDomain.CurrentDomain.BaseDirectory + "TestApp.exe");
            testApp.InjectHookerLib(serverPort);

            using (var client = new HookedClient(tcpServer.AcceptTcpClient()))
            {
                //client.ShowMessageBox("Hello World!");
                var methodsToHook = new[]
                {
                    ApiDefinitions.SetConsoleTitleA,
                    ApiDefinitions.SetConsoleWindowInfo,
                    ApiDefinitions.SetConsoleScreenBufferSize,
                    ApiDefinitions.WriteConsoleOutputA,
                    ApiDefinitions.GetConsoleTitleA
                };

                foreach (var m in methodsToHook)
                    m.SaveCallback = true;

                var hookedMethods = client.HookFuncs(methodsToHook);

                VsDebuggerHelper.AttachToProcess(testApp.Process.Id);

                testApp.ResumeMainThread();

                Thread.Sleep(500);

                var buffer = client.ReadBuffer();
                var modules = ProcessHelper.GetProcessModules(testApp.Process.Id);

                var callRecs = SerializationHelper.ProcessCallRecords(buffer, hookedMethods);
                foreach (var callRec in callRecs)
                {
                    Console.WriteLine(callRec);

                    foreach (var item in callRec.CallStack)
                    {
                        item.Module = modules.FirstOrDefault(x => x.BaseAddr <= item.Address && item.Address < x.EndAddr);
                        if (item.Module != null)
                            Console.WriteLine($" - {item.Module.Name}!0x{item.Address - item.Module.BaseAddr:x}");
                        else
                            Console.WriteLine($" - 0x{item.Address:x8}");
                    }

                    Console.WriteLine();
                }

                client.TerminateInjectionThread();
                testApp.Process.WaitForExit();
            }
        }

        static void Main(string[] args)
        {
            //JsonRpcTest.TestRpc();
            UIApiAsync(CancellationToken.None);

            Console.WriteLine("UI API active. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
