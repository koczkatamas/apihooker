using System.Collections.Generic;
using System.Linq;

namespace ApiHooker.Model.StructSerialization
{
    public class FieldDescriptor
    {
        public string Name { get; set; }
        public string TypeStr { get; set; }

        public FieldType Type { get; set; }
        public int Length { get; set; }
        public bool IsArray { get; set; }
        public byte PointerLevel { get; set; }
        public List<FieldDescriptor> Fields { get; set; } = new List<FieldDescriptor>();

        public FieldDescriptor() { }

        public FieldDescriptor(FieldDescriptor other)
        {
            Name = other.Name;
            TypeStr = other.TypeStr;
            Type = other.Type;
            Length = other.Length;
            IsArray = other.IsArray;
            PointerLevel = other.PointerLevel;
            Fields = other.Fields.ToList();
        }

        public override string ToString()
        {
            return $"{TypeStr} {Name}";
        }
    }
}