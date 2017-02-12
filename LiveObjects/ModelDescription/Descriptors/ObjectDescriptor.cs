using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            context.PublishObject((ILiveObject) value);

            foreach (var prop in Properties.Values)
            {
                var propVal = prop.PropertyInfo.GetValue(value);
                if (propVal is IEnumerable)
                    foreach(var liveObj in ((IEnumerable)propVal).OfType<ILiveObject>())
                        context.PublishObject(liveObj);

                if(propVal is ILiveObject)
                    context.PublishObject((ILiveObject)propVal);
            }

            return base.Serialize(context, value);
        }
    }
}