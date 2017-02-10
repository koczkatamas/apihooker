using System;
using Newtonsoft.Json;

namespace ApiHooker.UiApi.JsonRpc
{
    public class RpcDataType
    {
        [JsonIgnore]
        public Type TypeInfo { get; set; }

        public string CsTypeName => TypeInfo.Name;

        public virtual object Parse(object value) => value;
        public virtual object Serialize(object value) => value;
    }
}