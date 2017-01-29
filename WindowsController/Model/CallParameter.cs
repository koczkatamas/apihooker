using System;
using ApiHooker.Model.StructSerialization;
using ApiHooker.Utils.ExtensionMethods;

namespace ApiHooker.Model
{
    public class CallParameter
    {
        public FieldDescriptor FieldDescriptor { get; set; }
        //public object Value { get; set; }
        public string StringValue { get; set; }
        public byte[] Bytes { get; set; }
        public UInt64 IntValue { get; set; }

        public CallParameter[] SubFields { get; set; }

        static UInt64 BytesToInt(byte[] bytes)
        {
            UInt64 result = 0, multi = 1;
            foreach (var b in bytes)
            {
                result += b * multi;
                multi *= 256;
            }
            return result;
        }

        public override string ToString()
        {
            if (StringValue != null)
                return $@"""{StringValue}""";
            else if (SubFields?.Length > 0)
                return $"{{ {SubFields.Join(", ")} }}";
            else if (Bytes != null && Bytes.Length <= 8)
                return BytesToInt(Bytes).ToString();
            else
                return "???";
        }
    }
}