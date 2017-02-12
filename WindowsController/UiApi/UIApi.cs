using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ApiHooker.Communication;
using ApiHooker.Model;
using ApiHooker.Utils;
using ApiHooker.VisualStudio;
using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class AppModel: ILiveObject
    {
        public string ResourceId => "api";

        [Publish]
        public ObservableCollection<HookedProcess> HookedProcesses { get; protected set; } = new ObservableCollection<HookedProcess>();

        [Publish]
        public string Echo(string message) => $"echo response: {message}";

        private TcpListener tcpServer;
        private int tcpServerPort = 1337;

        public void Init()
        {
            tcpServer = new TcpListener(IPAddress.Loopback, tcpServerPort);
            tcpServer.Start();
        }

        public HookedProcess LaunchAndInject(string path)
        {
            var p = new HookedProcess();

            p.ProcessManager = ProcessManager.LaunchSuspended(path);
            p.ProcessManager.InjectHookerLib(tcpServerPort);

            p.HookedClient = new HookedClient(tcpServer.AcceptTcpClient());

            return p;
        }

        [Publish]
        public HookedProcess LaunchAndHook(string path)
        {
            var p = LaunchAndInject(path);

            var methodsToHook = HookableMethods.Where(x => x.HookIt).Select(x => new HookedMethod(x.ApiMethod) { SaveCallback = true }).ToArray();
            p.HookedMethods = p.HookedClient.HookFuncs(methodsToHook);

            p.ProcessManager.ResumeMainThread();

            return p;
        }

        [Publish]
        public ObservableCollection<UIHookableMethod> HookableMethods { get; protected set; }

        public AppModel()
        {
            HookableMethods = new ObservableCollection<UIHookableMethod>(typeof(ApiDefinitions).GetFields()
                .Where(x => x.FieldType == typeof(ApiMethod)).Select(f => new UIHookableMethod((ApiMethod)f.GetValue(null))).ToArray());
        }
    }
}