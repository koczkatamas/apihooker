using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ApiHooker.Model;
using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class UIApi: ILiveObject
    {
        public string ResourceId => "api";

        public UIProcess CurrentProcess { get; protected set; }

        [Publish]
        public async Task<string> EchoAsync(string message) => $"echo response: {message}";

        [Publish]
        public async Task<UIProcess> LaunchAndInjectAsync(string path)
        {
            if (CurrentProcess?.ProcessManager.Process?.HasExited != false)
                throw new Exception("Process already running!");

            return new UIProcess(ProcessManager.LaunchSuspended(path));
        }

        [Publish]
        public ObservableCollection<UIHookableMethod> HookableMethods { get; protected set; }

        public UIApi()
        {
            HookableMethods = new ObservableCollection<UIHookableMethod>(typeof(ApiDefinitions).GetFields()
                .Where(x => x.FieldType == typeof(ApiMethod)).Select(f => new UIHookableMethod((ApiMethod)f.GetValue(null))).ToArray());
        }
    }
}