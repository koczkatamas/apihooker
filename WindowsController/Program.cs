using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using ApiHooker.Utils;
using ApiHooker.VisualStudio;
using LiveObjects.Communication;
using LiveObjects.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;

namespace ApiHooker
{
    class Program
    {
        static void Main(string[] args)
        {
            //var uiApi = new UIApi();
            //var jsonRpc = new JsonRpc();
            //jsonRpc.PublishObject(uiApi);
            //
            //var typeInfos = jsonRpc.TypeInformation;
            //var typeInfosJson = JsonConvert.SerializeObject(typeInfos.Select(x => x.Value), Formatting.Indented);

            //JsonRpcTest.TestRpc();
            new Program().Start();

            Console.WriteLine("UI API active. Press ENTER to exit.");
            Console.ReadLine();
        }

        public AppModel AppModel { get; protected set; }

        private void Start()
        {
            AppModel = new AppModel();
            AppModel.Init();
            UIApiAsync(CancellationToken.None);
            TestApp();
        }

        void TestApp()
        {
            foreach (var hm in AppModel.HookableMethods)
                hm.HookIt = true;

            var testApp = AppModel.LaunchAndInject("TestApp.exe");

            Thread.Sleep(500);

            var callRecs = testApp.ReadNewCallRecords();

            foreach (var callRec in callRecs)
            {
                Console.WriteLine(callRec);

                foreach (var item in callRec.CallStack)
                    if (item.Module != null)
                        Console.WriteLine($" - {item.Module.Name}!0x{item.Address - item.Module.BaseAddr:x}");
                    else
                        Console.WriteLine($" - 0x{item.Address:x8}");

                Console.WriteLine();
            }

            testApp.UnhookAndWaitForExit();
        }


        static async Task WsPublishAsync(IPEndPoint endPoint, MessageBridge bridge, string[] allowedOrigins, ILogger logger = null, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var webSocket = new WebSocketListener(endPoint);
                webSocket.Standards.RegisterStandard(new WebSocketFactoryRfc6455(webSocket));
                webSocket.Start();

                logger?.Log("Started.");

                while (!ct.IsCancellationRequested)
                {
                    var client = await webSocket.AcceptWebSocketAsync(ct);

                    try
                    {
                        var origin = client.HttpRequest.Headers["Origin"];
                        if (!allowedOrigins.Contains(origin))
                        {
                            await client.WriteStringAsync(@"{ ""Error"": ""NotAllowedOrigin"" }", ct);
                            client.Close();
                            continue;
                        }

                        while (client.IsConnected)
                        {
                            var request = await client.ReadStringAsync(ct);
                            if (request == null) break;
                            var response = await bridge.ProcessMessageAsync(request);
                            await client.WriteStringAsync(response, ct);
                        }
                    }
                    catch (Exception clientExc)
                    {
                        logger?.LogException(new Exception("WebSocket Client Exception", clientExc));
                    }
                }

                logger?.Log("Stopped.");
            }
            catch (Exception e)
            {
                logger?.LogException(new Exception("WebSocket Client Exception", e));
            }
        }

        async Task UIApiAsync(CancellationToken ct)
        {
            var logger = new ConsoleLogger { Prefix = "UIApi" };
            try
            {
                var jsonRpc = new MessageBridge();
                jsonRpc.ObjectContext.PublishObject(AppModel);

                await WsPublishAsync(new IPEndPoint(IPAddress.Loopback, 1338), jsonRpc, new [] { "http://127.0.0.1:8000" }, logger, ct);
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
        }
    }
}
