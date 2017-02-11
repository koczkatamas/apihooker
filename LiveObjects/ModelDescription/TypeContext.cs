using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LiveObjects.Utils.ExtensionMethods;

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

                var optOut = type.GetCustomAttribute<PublicationPolicyAttribute>()?.DefaultPublicationMode == DefaultPublicationMode.OptOut;
                foreach (var methodInfo in type.GetMethods())
                {
                    var include = optOut ? methodInfo.GetCustomAttribute<DoNotPublishAttribute>() == null : methodInfo.GetCustomAttribute<PublishAttribute>() != null;
                    if (!include) continue;

                    var isAsync = typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
                    var name = isAsync ? methodInfo.Name.RemovePostfix("Async") : methodInfo.Name;

                    var methodDesc = new MethodDescriptor { MethodInfo = methodInfo, Name = name, Async = isAsync };

                    var parameterInfos = methodInfo.GetParameters();
                    foreach (var parameterInfo in parameterInfos)
                    {
                        var paramDesc = GetTypeDescriptor(parameterInfo.ParameterType);
                        methodDesc.Parameters.Add(new MethodParameterDescriptor { ParameterInfo = parameterInfo, Type = paramDesc });
                    }

                    if (isAsync)
                    {
                        var resultGenArgs = methodInfo.ReturnType.GetGenericArguments();
                        methodDesc.ResultType = resultGenArgs.Length > 0 ? GetTypeDescriptor(resultGenArgs[0]) : null;
                    }
                    else
                        methodDesc.ResultType = GetTypeDescriptor(methodInfo.ReturnType);

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