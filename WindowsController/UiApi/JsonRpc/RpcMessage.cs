using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiHooker.UiApi.JsonRpc
{
    public class RpcMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RpcMessageType MessageType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RpcMessageError Error { get; set; }

        public string MessageId { get; set; }

        public string ResourceId { get; set; }

        public string MethodName { get; set; }

        public List<object> Arguments { get; set; }

        public object Result { get; set; }

        public static RpcMessage CreateError(RpcMessageError error) => new RpcMessage { MessageType = RpcMessageType.Error, Error = error };
    }
}