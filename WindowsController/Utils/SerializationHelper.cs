using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ApiHooker.Communication;
using ApiHooker.Model;
using ApiHooker.Model.StructSerialization;

namespace ApiHooker.Utils
{
    public static class SerializationHelper
    {
        static CallParameter ReadCallParameter(BinaryReader br, FieldDescriptor fieldDesc)
        {
            var result = new CallParameter { FieldDescriptor = fieldDesc };

            if (fieldDesc.Type == FieldType.NullTerminatedAnsiString)
            {
                var strLen = br.ReadInt32();
                result.StringValue = Encoding.Default.GetString(br.ReadBytes(strLen));
            }
            else if (fieldDesc.Type == FieldType.ByteArray)
            {
                if (fieldDesc.Length == -1) throw new Exception("Invalid Field Descriptor!");
                result.Bytes = br.ReadBytes(fieldDesc.Length);
            }
            else if (fieldDesc.Type == FieldType.Struct)
            {
                if (fieldDesc.Fields.Count == 0) throw new Exception("Invalid Field Descriptor!");
                result.SubFields = ReadCallParameters(br, fieldDesc.Fields);
            }

            return result;
        }

        static CallParameter[] ReadCallParameters(BinaryReader br, IEnumerable<FieldDescriptor> fieldDescs)
        {
            return fieldDescs.Select(fieldDesc => ReadCallParameter(br, fieldDesc)).ToArray();
        }

        public static CallRecord[] ProcessCallRecords(byte[] data, HookedClient.HookedMethods hookedMethods = null)
        {
            var result = new List<CallRecord>();

            var br = new BinaryReader(new MemoryStream(data));
            while (br.BaseStream.Position != br.BaseStream.Length)
            {
                var cr = new CallRecord();
                cr.RecordSize = br.ReadUInt32();
                cr.RecordData = br.ReadBytes((int)cr.RecordSize);

                var brCr = new BinaryReader(new MemoryStream(cr.RecordData));
                cr.CallId = brCr.ReadUInt32();
                cr.FunctionId = brCr.ReadUInt32();
                cr.ThreadId = brCr.ReadUInt32();

                if (hookedMethods != null)
                {
                    cr.ApiMethod = hookedMethods.MethodIds[cr.FunctionId];
                    cr.Parameters = ReadCallParameters(brCr, cr.ApiMethod.Arguments);
                }

                result.Add(cr);
            }

            return result.ToArray();
        }

        public static void SerializeFieldDescriptors(BinaryWriter bw, FieldDescriptor[] fields)
        {
            bw.Write((UInt32)fields.Length);
            foreach (var field in fields)
            {
                bw.Write((UInt32)field.Type);
                bw.Write((UInt32)field.Length);
                bw.Write((byte)(field.IsArray ? 1 : 0));
                bw.Write(field.PointerLevel);
                SerializeFieldDescriptors(bw, field.Fields.ToArray());
            }
        }

        public static byte[] SerializeFieldDescriptors(FieldDescriptor[] fields)
        {
            var ms = new MemoryStream();
            SerializeFieldDescriptors(new BinaryWriter(ms), fields);
            return ms.ToArray();
        }
    }
}