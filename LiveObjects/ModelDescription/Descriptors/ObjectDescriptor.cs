using System;
using System.Collections.Generic;
using LiveObjects.ObjectContext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiveObjects.ModelDescription
{
    public class ObjectDescriptor: TypeDescriptor
    {
        public Dictionary<string, MethodDescriptor> Methods { get; protected set; } = new Dictionary<string, MethodDescriptor>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, PropertyDescriptor> Properties { get; protected set; } = new Dictionary<string, PropertyDescriptor>(StringComparer.OrdinalIgnoreCase);

        public override object Parse(IObjectContext context, object value)
        {
            var resourceId = (string)((JObject)value)["ResourceId"];
            var result = context.GetObject(resourceId);
            return result;
        }

        public override object Serialize(IObjectContext context, object value)
        {
            context.PublishObject((IUIObject) value);
            return base.Serialize(context, value);
        }
    }
}