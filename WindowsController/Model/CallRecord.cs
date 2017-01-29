using System;
using System.Collections.Generic;
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
        public CallParameter[] ParametersBeforeCall { get; set; }
        public UInt32 ReturnValue { get; set; }
        public CallParameter[] ParametersAfterCall { get; set; }
        public List<CallStackEntry> CallStack { get; set; }

        public override string ToString()
        {
            return $"{ApiMethod?.MethodName ?? $"Func #{FunctionId}"}({ParametersAfterCall?.Join(", ")}) = {ReturnValue}";
        }
    }
}