using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace LiveObjects.ModelDescription.Helpers
{
    public class LiveObjectResolver : DefaultContractResolver
    {
        public TypeContext TypeContext { get; protected set; }

        public LiveObjectResolver(TypeContext typeContext)
        {
            TypeContext = typeContext;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var objectDescriptor = TypeContext.GetTypeDescriptor(objectType, false) as ObjectDescriptor;
            if (objectDescriptor == null)
                return base.GetSerializableMembers(objectType);
            return objectDescriptor.Properties.Select(x => (MemberInfo) x.Value.PropertyInfo).ToList();
        }
    }
}