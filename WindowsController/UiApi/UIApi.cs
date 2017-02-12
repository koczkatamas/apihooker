using System.Linq;
using System.Threading.Tasks;
using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class UIApi: ILiveObject
    {
        public string ResourceId => "api";

        [Publish]
        public async Task<string> EchoAsync(string message) => $"echo response: {message}";

        [Publish]
        public async Task<UIProcess> LaunchAndInjectAsync(string path)
        {
            return new UIProcess { Path = path };
        }

        [Publish]
        public async Task<UIHookableMethod[]> GetHookableMethodsAsync()
        {
            return typeof(ApiDefinitions).GetFields().Select(f => new UIHookableMethod { Name = f.Name }).ToArray();
        }
    }
}