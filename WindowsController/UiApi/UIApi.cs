using System.Linq;
using System.Threading.Tasks;
using ApiHooker.UiApi.JsonRpc;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UIApi: IUIObject
    {
        [JsonProperty]
        public string ResourceId => "api";

        public async Task<string> EchoAsync(string message) => $"echo response: {message}";

        public async Task<UIProcess> LaunchAndInjectAsync(string path)
        {
            return new UIProcess { Path = path };
        }

        public async Task<UIHookableMethod[]> GetHookableMethodsAsync()
        {
            return typeof(ApiDefinitions).GetFields().Select(f => new UIHookableMethod { Name = f.Name }).ToArray();
        }
    }
}