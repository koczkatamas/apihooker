using System;
using System.Collections;
using System.Linq;
using LiveObjects.ObjectContext;
using Newtonsoft.Json.Linq;

namespace LiveObjects.ModelDescription
{
    public class ArrayDescriptor: TypeDescriptor
    {
        public TypeDescriptor ArrayItemType { get; set; }

        public override object Parse(IObjectContext context, object value)
        {
            var jsonArray = (JArray)value;

            var resultArray = Array.CreateInstance(ArrayItemType.TypeInfo, jsonArray.Count);
            for (var i = 0; i < jsonArray.Count; i++)
            {
                var arrayItem = ArrayItemType.Parse(context, jsonArray[i]);
                resultArray.SetValue(arrayItem, i);
            }

            return resultArray;
        }

        public override object Serialize(IObjectContext context, object value)
        {
            return ((IEnumerable) value).Cast<object>().Select(x => ArrayItemType.Serialize(context, x)).ToArray();
        }
    }
}