using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace LiveObjects.ModelDescription
{
    public class MethodDescriptor
    {
        [JsonIgnore]
        public MethodInfo MethodInfo { get; set; }

        public string Name { get; set; }
        public List<MethodParameterDescriptor> Parameters { get; set; } = new List<MethodParameterDescriptor>();
        public TypeDescriptor ResultType { get; set; }
        public bool Async { get; set; }
    }
}