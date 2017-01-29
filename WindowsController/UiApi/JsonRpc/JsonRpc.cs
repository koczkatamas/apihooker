using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiHooker.Utils.ExtensionMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiHooker.UiApi.JsonRpc
{
    public class JsonRpc
    {
        public Dictionary<string, IUIObject> ObjectRepository { get; protected set; } = new Dictionary<string, IUIObject>();

        public void PublishObject(IUIObject obj)
        {
            var objId = obj.ResourceId;
            var existingObj = ObjectRepository.GetValueOrDefault(objId);
            if (existingObj != null && existingObj != obj)
                throw new Exception($"Object's key is not unique: {objId}");
            ObjectRepository[objId] = obj;
        }

        private IUIObject GetObject(string resourceId)
        {
            var obj = ObjectRepository.GetValueOrDefault(resourceId ?? "");
            if (obj == null)
                throw new RpcMessageException(RpcMessageError.ResourceNotFound);
            return obj;
        }

        private object ConvertObject(Type expectedType, object value)
        {
            if (expectedType == typeof(string) && value is string)
                return value;
            else if (expectedType.IsArray && value is JArray)
            {
                var jsonArray = (JArray) value;
                var arrayItemType = expectedType.GetElementType();
                var resultArray = Array.CreateInstance(arrayItemType, jsonArray.Count);
                for (int i = 0; i < jsonArray.Count; i++)
                    resultArray.SetValue(ConvertObject(arrayItemType, jsonArray[i]), i);
                return resultArray;
            }
            else if (value is JObject)
            {
                var resourceId = (string) ((JObject) value)["ResourceId"];
                var result = GetObject(resourceId);
                return result;
            }

            throw new RpcMessageException(RpcMessageError.UnknownArgumentType);
        }

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

                    var method = obj.GetType().GetMethods().FirstOrDefault(x => typeof(Task).IsAssignableFrom(x.ReturnType) && x.Name.Replace("Async", "").ToLower() == requestObj.MethodName?.ToLower());
                    if (method == null)
                        throw new RpcMessageException(RpcMessageError.MethodNotFound);

                    var parameterInfos = method.GetParameters();
                    if (parameterInfos.Length != (requestObj.Arguments?.Count ?? 0))
                        throw new RpcMessageException(RpcMessageError.ArgumentCountMismatch);

                    var parameters = new List<object>();
                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        var expectedType = parameterInfos[i].ParameterType;
                        var value = requestObj.Arguments?[i];

                        var converted = ConvertObject(expectedType, value);
                        parameters.Add(converted);
                    }

                    var task = (Task) method.Invoke(obj, parameters.ToArray());
                    await task;

                    if (task.GetType().GetGenericArguments()[0].Name != "VoidTaskResult")
                        responseObj.Result = ((dynamic) task).Result;

                    responseObj.Error = RpcMessageError.NoError;
                    responseObj.MessageType = RpcMessageType.CallResponse;

                    if (responseObj.Result is IEnumerable)
                        foreach(var item in ((IEnumerable)responseObj.Result).OfType<IUIObject>())
                            PublishObject(item);

                    var resultObject = responseObj.Result as IUIObject;
                    if (resultObject != null)
                        PublishObject(resultObject);
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