using System;
using LiveObjects.ObjectContext;
using Newtonsoft.Json;

namespace LiveObjects.ModelDescription
{
    public class TypeDescriptor
    {
        [JsonIgnore]
        public Type TypeInfo { get; set; }

        public string CsTypeName => TypeInfo.Name;

        public virtual object Parse(IObjectContext context, object value) => value;
        public virtual object Serialize(IObjectContext context, object value) => value;
    }
}