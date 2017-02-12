using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ApiHooker.Communication;
using ApiHooker.Model;
using ApiHooker.Utils;
using ApiHooker.VisualStudio;
using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class HookedProcess : ILiveObject
    {
        public ProcessManager ProcessManager { get; set; }
        public HookedClient HookedClient { get; set; }
        public HookedClient.HookedMethods HookedMethods { get; set; }

        public ObservableRangeCollection<CallRecord> CallRecords { get; set; } = new ObservableRangeCollection<CallRecord>();

        [Publish]
        public string ResourceId => $"process/{Name}";

        [Publish]
        public string Name => ProcessManager.Process.ProcessName;

        [Publish]
        public CallRecord[] ReadNewCallRecords()
        {
            var buffer = HookedClient.ReadBuffer();
            var modules = ProcessHelper.GetProcessModules(ProcessManager.Process.Id);

            var callRecs = SerializationHelper.ProcessCallRecords(buffer, HookedMethods);
            foreach (var callRec in callRecs)
                foreach (var item in callRec.CallStack)
                    item.Module = modules.FirstOrDefault(x => x.BaseAddr <= item.Address && item.Address < x.EndAddr);

            CallRecords.AddRange(callRecs);
            return callRecs;
        }

        [Publish]
        public void UnhookAndWaitForExit()
        {
            HookedClient.TerminateInjectionThread();
            ProcessManager.Process.WaitForExit();
        }
    }
}