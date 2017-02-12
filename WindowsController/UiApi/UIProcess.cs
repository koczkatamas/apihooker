using System.Diagnostics;
using System.Threading.Tasks;
using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class UIProcess: ILiveObject
    {
        public ProcessManager ProcessManager { get; protected set; }
        public Process Process { get; protected set; }

        public UIProcess(ProcessManager processManager)
        {
            ProcessManager = processManager;
            Process = processManager.Process;
        }

        [Publish]
        public string ResourceId => $"process/{Name}";

        [Publish]
        public string Name => Process.ProcessName;

        [Publish]
        public async Task HookMethodsAsync(UIHookableMethod[] methods)
        {
            
        }

        [Publish]
        public async Task ResumeMainThreadAsync()
        {
            
        }
    }
}