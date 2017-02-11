using System.Reflection;
using Newtonsoft.Json;

namespace LiveObjects.ModelDescription
{
    public class MethodParameterDescriptor
    {
        [JsonIgnore]
        public ParameterInfo ParameterInfo { get; set; }

        public string Name => ParameterInfo?.Name;
        public TypeDescriptor Type { get; set; }
    }
}