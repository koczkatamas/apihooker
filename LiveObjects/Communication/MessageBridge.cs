using System;
using System.Linq;
using System.Threading.Tasks;
using LiveObjects.DependencyInjection;
using LiveObjects.Logging;
using LiveObjects.ModelDescription;
using LiveObjects.ModelDescription.Helpers;
using LiveObjects.Utils.ExtensionMethods;
using Newtonsoft.Json;

namespace LiveObjects.Communication
{
    public delegate void ChangeMessageEvent(MessageBridge bridge, Message changeMessage);

    public class MessageBridge
    {
        public ILogger Logger { get; } = Dependency.Get<ILogger>(false);
        public ObjectContext.ObjectContext ObjectContext { get; set; } = new ObjectContext.ObjectContext();
        public event ChangeMessageEvent ChangeMessageEvent;

        public MessageBridge()
        {
            ObjectContext.ObjectPropertyChanged += (objContext, liveObj, propName) =>
            {
                var objectDesc = (ObjectDescriptor) ObjectContext.TypeContext.GetTypeDescriptor(liveObj.GetType(), false);
                var newValue = objectDesc.Properties[propName].PropertyInfo.GetValue(liveObj);
                ChangeMessageEvent?.Invoke(this, new Message
                {
                    Error = MessageError.NoError,
                    MessageType = MessageType.PropertyChanged,
                    ResourceId = liveObj.ResourceId,
                    PropertyName = propName,
                    Value = newValue
                });
            };
        }

        public async Task<Message> ProcessMessageAsync(Message request)
        {
            var response = new Message { Error = MessageError.UnexpectedError };

            try
            {
                response.MessageId = request.MessageId;

                if (request.MessageType != MessageType.Call && request.MessageType != MessageType.Get && request.MessageType != MessageType.SetProperty)
                    throw new MessageException(MessageError.UnknownMessageType);

                var obj = ObjectContext.GetObject(request.ResourceId);
                if (obj == null)
                    throw new MessageException(MessageError.ResourceNotFound);

                var typeInfo = (ObjectDescriptor)ObjectContext.TypeContext.GetTypeDescriptor(obj.GetType());

                if (request.MessageType == MessageType.Call)
                {
                    var method = typeInfo.Methods.GetValueOrDefault(request.MethodName);
                    if (method == null)
                        throw new MessageException(MessageError.MethodNotFound);

                    if (method.Parameters.Count != (request.Arguments?.Count ?? 0))
                        throw new MessageException(MessageError.ArgumentCountMismatch);

                    var parameters = method.Parameters.Select((x, i) => x.Type.Parse(ObjectContext, request.Arguments?[i])).ToArray();
                    var result = method.MethodInfo.Invoke(obj, parameters);
                    if (method.Async)
                    {
                        var task = (Task) result;
                        await task;
                        result = ((dynamic) task).Result;
                    }

                    if (method.ResultType != null)
                        response.Value = method.ResultType.Serialize(ObjectContext, result);

                    response.Error = MessageError.NoError;
                    response.MessageType = MessageType.CallResponse;
                }
                else if (request.MessageType == MessageType.Get)
                {
                    response.Error = MessageError.NoError;
                    response.MessageType = MessageType.GetResponse;
                    response.Value = obj;
                }
                else if (request.MessageType == MessageType.SetProperty)
                {
                    var propDesc = typeInfo.Properties.GetValueOrDefault(request.PropertyName);
                    if (propDesc == null)
                        throw new MessageException(MessageError.PropertyNotFound);

                    propDesc.PropertyInfo.SetValue(obj, request.Value);

                    response.Error = MessageError.NoError;
                    response.MessageType = MessageType.SuccessConfirmation;
                }
                else
                    throw new MessageException(MessageError.UnknownMessageType);
            }
            catch (MessageException e)
            {
                response.Error = e.Error;
            }
            catch (Exception e)
            {
                Logger?.LogException(new Exception("Unexpected error while processing LiveObject request message", e));
            }

            return response;
        }

        public async Task<string> ProcessMessageAsync(string request)
        {
            Message requestObj = null, responseObj = null;
            try
            {
                requestObj = JsonConvert.DeserializeObject<Message>(request);
            }
            catch (Exception e)
            {
                responseObj = new Message { Error = MessageError.RequestParsingError };
                Logger?.LogException(new Exception("Could not parse LiveObject request JSON message", e));
            }

            if(requestObj != null)
                responseObj = await ProcessMessageAsync(requestObj);

            return JsonConvert.SerializeObject(responseObj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new LiveObjectResolver(ObjectContext.TypeContext)
            });
        }
    }
}