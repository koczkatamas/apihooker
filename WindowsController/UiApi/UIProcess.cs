using System.Threading.Tasks;
using ApiHooker.UiApi.JsonRpc;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UIProcess: IUIObject
    {
        [JsonProperty]
        public string ResourceId => $"process/{Path}";

        [JsonProperty]
        public string Path { get; set; }

        public async Task HookMethodsAsync(UIHookableMethod[] methods)
        {
            
        }

        public async Task ResumeMainThreadAsync()
        {
            
        }
    }
}