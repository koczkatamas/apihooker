using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiHooker.UiApi.JsonRpc
{
    public class RpcObject: RpcDataType
    {
        [JsonIgnore]
        public IObjectRepository ObjectRepository { get; set; }

        public Dictionary<string, RpcMethod> Methods { get; protected set; } = new Dictionary<string, RpcMethod>(StringComparer.OrdinalIgnoreCase);

        public override object Parse(object value)
        {
            var resourceId = (string)((JObject)value)["ResourceId"];
            var result = ObjectRepository.GetObject(resourceId);
            return result;
        }

        public override object Serialize(object value)
        {
            ObjectRepository.PublishObject((IUIObject) value);
            return base.Serialize(value);
        }
    }
}