using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ApiHooker.UiApi.JsonRpc
{
    public class RpcArray: RpcDataType
    {
        public RpcDataType ArrayItemType { get; set; }

        public override object Parse(object value)
        {
            var jsonArray = (JArray)value;

            var resultArray = Array.CreateInstance(ArrayItemType.TypeInfo, jsonArray.Count);
            for (var i = 0; i < jsonArray.Count; i++)
            {
                var arrayItem = ArrayItemType.Parse(jsonArray[i]);
                resultArray.SetValue(arrayItem, i);
            }

            return resultArray;
        }

        public override object Serialize(object value)
        {
            return ((IEnumerable) value).Cast<object>().Select(x => ArrayItemType.Serialize(x)).ToArray();
        }
    }
}