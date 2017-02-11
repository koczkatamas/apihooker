using System.Reflection;
using Newtonsoft.Json;

namespace LiveObjects.ModelDescription
{
    public class PropertyDescriptor
    {
        [JsonIgnore]
        public PropertyInfo PropertyInfo { get; set; }

        public string Name => PropertyInfo.Name;
        public string CsTypeName => PropertyInfo.PropertyType.ToString();
    }
}