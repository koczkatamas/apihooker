using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LiveObjects.Communication
{
    public class ListChangeData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ListChangeAction Action { get; set; }

        public int NewStartingIndex { get; set; }
        public int OldStartingIndex { get; set; }
        public IList NewItems { get; set; }
        public IList OldItems { get; set; }
    }
}