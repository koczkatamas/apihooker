using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LiveObjects.Communication
{
    public class ListChangeItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ListChangeAction Action { get; set; }

        public int Index { get; set; }
        public object Value { get; set; }
    }
}