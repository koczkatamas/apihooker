using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace ApiHooker.UiApi.JsonRpc
{
    public class RpcMethod
    {
        [JsonIgnore]
        public MethodInfo MethodInfo { get; set; }

        public string Name { get; set; }
        public List<RpcMethodParameter> Parameters { get; set; } = new List<RpcMethodParameter>();
        public RpcDataType ResultType { get; set; }
    }
}