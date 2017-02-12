using System.Threading.Tasks;
using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class UIProcess: ILiveObject
    {
        [Publish]
        public string ResourceId => $"process/{Path}";

        [Publish]
        public string Path { get; set; }

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