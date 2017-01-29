using System;
using ApiHooker.Utils.ExtensionMethods;

namespace ApiHooker.Model
{
    public class CallRecord
    {
        public UInt32 RecordSize { get; set; }
        public byte[] RecordData { get; set; }

        public UInt32 CallId { get; set; }
        public UInt32 FunctionId { get; set; }
        public UInt32 ThreadId { get; set; }

        public ApiMethod ApiMethod { get; set; }
        public CallParameter[] Parameters { get; set; }

        public override string ToString()
        {
            return $"{ApiMethod?.MethodName ?? $"Func #{FunctionId}"}({Parameters?.Join(", ")})";
        }
    }
}