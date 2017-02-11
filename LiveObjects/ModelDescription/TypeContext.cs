using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveObjects.ModelDescription
{
    public class TypeContext
    {
        public static TypeContext Instance { get; set; } = new TypeContext();

        public ConcurrentDictionary<Type, TypeDescriptor> TypeDescriptors { get; protected set; } = new ConcurrentDictionary<Type, TypeDescriptor>();

        private TypeDescriptor CreateTypeDescriptor(Type type)
        {
            TypeDescriptor result;

            if (type.IsArray)
                result = new ArrayDescriptor { TypeInfo = type, ArrayItemType = GetTypeDescriptor(type.GetElementType()) };
            else if (typeof(IUIObject).IsAssignableFrom(type))
            {
                var objDesc = new ObjectDescriptor { TypeInfo = type };

                foreach (var methodInfo in type.GetMethods().Where(methodInfo => typeof(Task).IsAssignableFrom(methodInfo.ReturnType)))
                {
                    var methodDesc = new MethodDescriptor { MethodInfo = methodInfo, Name = methodInfo.Name.Replace("Async", "") };

                    var parameterInfos = methodInfo.GetParameters();
                    foreach (var parameterInfo in parameterInfos)
                    {
                        var paramDesc = GetTypeDescriptor(parameterInfo.ParameterType);
                        methodDesc.Parameters.Add(new MethodParameterDescriptor { ParameterInfo = parameterInfo, Type = paramDesc });
                    }

                    var resultGenArgs = methodInfo.ReturnType.GetGenericArguments();
                    methodDesc.ResultType = resultGenArgs.Length > 0 ? GetTypeDescriptor(resultGenArgs[0]) : null;

                    objDesc.Methods[methodDesc.Name] = methodDesc;
                }

                result = objDesc;
            }
            else
                result = new TypeDescriptor { TypeInfo = type };

            return result;
        }

        public TypeDescriptor GetTypeDescriptor(Type type)
        {
            TypeDescriptor result;

            if (!TypeDescriptors.TryGetValue(type, out result))
                result = TypeDescriptors[type] = CreateTypeDescriptor(type);

            return result;
        }
    }
}