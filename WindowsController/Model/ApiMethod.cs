using System;
using System.Collections.Generic;
using ApiHooker.Model.StructSerialization;

namespace ApiHooker.Model
{
    public class ApiMethod
    {
        public string DllName { get; set; }
        public string MethodName { get; set; }
        public List<FieldDescriptor> Arguments { get; set; } = new List<FieldDescriptor>();

        public override string ToString()
        {
            return $"{DllName}!{MethodName}({String.Join(", ", Arguments)})";
        }
    }
}