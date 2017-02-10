using System.Reflection;
using Newtonsoft.Json;

namespace ApiHooker.UiApi.JsonRpc
{
    public class RpcMethodParameter
    {
        [JsonIgnore]
        public ParameterInfo ParameterInfo { get; set; }

        public string Name => ParameterInfo?.Name;
        public RpcDataType Type { get; set; }
    }
}