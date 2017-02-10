using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiHooker.Utils.ExtensionMethods;
using Newtonsoft.Json;

namespace ApiHooker.UiApi.JsonRpc
{
    public class JsonRpc: IObjectRepository
    {
        public Dictionary<Type, RpcObject> TypeInformation { get; protected set; } = new Dictionary<Type, RpcObject>();
        public Dictionary<string, IUIObject> ObjectRepository { get; protected set; } = new Dictionary<string, IUIObject>(StringComparer.OrdinalIgnoreCase);

        private RpcDataType GetRpcType(Type type)
        {
            if (type.IsArray)
                return new RpcArray { TypeInfo = type, ArrayItemType = GetRpcType(type.GetElementType()) };
            else if (typeof(IUIObject).IsAssignableFrom(type))
            {
                RpcObject rpcObjectType;
                if(TypeInformation.TryGetValue(type, out rpcObjectType))
                    return rpcObjectType;

                rpcObjectType = new RpcObject { TypeInfo = type, ObjectRepository = this };

                foreach (var methodInfo in type.GetMethods().Where(methodInfo => typeof(Task).IsAssignableFrom(methodInfo.ReturnType)))
                {
                    var rpcMethod = new RpcMethod { MethodInfo = methodInfo, Name = methodInfo.Name.Replace("Async", "") };

                    var parameterInfos = methodInfo.GetParameters();
                    foreach (var parameterInfo in parameterInfos)
                    {
                        var rpcParamType = GetRpcType(parameterInfo.ParameterType);
                        rpcMethod.Parameters.Add(new RpcMethodParameter { ParameterInfo = parameterInfo, Type = rpcParamType });
                    }

                    var resultGenArgs = methodInfo.ReturnType.GetGenericArguments();
                    rpcMethod.ResultType = resultGenArgs.Length > 0 ? GetRpcType(resultGenArgs[0]) : null;

                    rpcObjectType.Methods[rpcMethod.Name] = rpcMethod;
                }

                TypeInformation[type] = rpcObjectType;
                return rpcObjectType;
            }
            else
                return new RpcDataType { TypeInfo = type };
        }

        public void PublishObject(IUIObject obj)
        {
            var objId = obj.ResourceId;
            var existingObj = ObjectRepository.GetValueOrDefault(objId);
            if (existingObj != null && existingObj != obj)
                throw new Exception($"Object's key is not unique: {objId}");
            ObjectRepository[objId] = obj;

            GetRpcType(obj.GetType());
        }

        private IUIObject GetObject(string resourceId)
        {
            var obj = ObjectRepository.GetValueOrDefault(resourceId ?? "");
            if (obj == null)
                throw new RpcMessageException(RpcMessageError.ResourceNotFound);
            return obj;
        }

        IUIObject IObjectRepository.GetObject(string resourceId) => GetObject(resourceId);

        public async Task<string> ProcessMessageAsync(string request)
        {
            var responseObj = new RpcMessage { Error = RpcMessageError.UnexpectedError };

            try
            {
                var requestObj = JsonConvert.DeserializeObject<RpcMessage>(request);
                responseObj.MessageId = requestObj.MessageId;

                if (requestObj.MessageType == RpcMessageType.Call)
                {
                    var obj = GetObject(requestObj.ResourceId);
                    var typeInfo = TypeInformation[obj.GetType()];

                    var method = typeInfo.Methods.GetValueOrDefault(requestObj.MethodName);
                    if (method == null)
                        throw new RpcMessageException(RpcMessageError.MethodNotFound);

                    if (method.Parameters.Count != (requestObj.Arguments?.Count ?? 0))
                        throw new RpcMessageException(RpcMessageError.ArgumentCountMismatch);

                    var parameters = method.Parameters.Select((x, i) => x.Type.Parse(requestObj.Arguments?[i])).ToArray();
                    var task = (Task) method.MethodInfo.Invoke(obj, parameters);
                    await task;

                    if (method.ResultType != null)
                        responseObj.Result = method.ResultType.Serialize(((dynamic)task).Result);

                    responseObj.Error = RpcMessageError.NoError;
                    responseObj.MessageType = RpcMessageType.CallResponse;
                }
                else
                    throw new RpcMessageException(RpcMessageError.UnknownMessageType);
            }
            catch (RpcMessageException e)
            {
                responseObj.Error = e.Error;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while handling UI API request: {e}");
            }

            return JsonConvert.SerializeObject(responseObj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}