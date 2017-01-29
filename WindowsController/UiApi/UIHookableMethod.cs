using ApiHooker.UiApi.JsonRpc;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UIHookableMethod : IUIObject
    {
        [JsonProperty]
        public string ResourceId => $"hookableMethod/{Name}";

        [JsonProperty]
        public string Name { get; set; }
    }
}