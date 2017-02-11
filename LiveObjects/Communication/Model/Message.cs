using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LiveObjects.Communication
{
    public class Message
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType MessageType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MessageError Error { get; set; }

        public string MessageId { get; set; }

        public string ResourceId { get; set; }

        public string MethodName { get; set; }

        public List<object> Arguments { get; set; }

        public object Result { get; set; }

        public static Message CreateError(MessageError error) => new Message { MessageType = MessageType.Error, Error = error };
    }
}