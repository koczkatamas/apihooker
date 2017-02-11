using System;
using System.Linq;
using System.Threading.Tasks;
using LiveObjects.DependencyInjection;
using LiveObjects.Logging;
using LiveObjects.ModelDescription;
using LiveObjects.Utils.ExtensionMethods;
using Newtonsoft.Json;

namespace LiveObjects.Communication
{
    public class MessageBridge
    {
        public ILogger Logger { get; } = Dependency.Get<ILogger>(false);
        public ObjectContext.ObjectContext ObjectContext { get; set; } = new ObjectContext.ObjectContext();

        public async Task<Message> ProcessMessageAsync(Message request)
        {
            var response = new Message { Error = MessageError.UnexpectedError };

            try
            {
                response.MessageId = request.MessageId;

                if (request.MessageType == MessageType.Call)
                {
                    var obj = ObjectContext.GetObject(request.ResourceId);
                    if (obj == null)
                        throw new MessageException(MessageError.ResourceNotFound);

                    var typeInfo = (ObjectDescriptor)ObjectContext.TypeContext.GetTypeDescriptor(obj.GetType());

                    var method = typeInfo.Methods.GetValueOrDefault(request.MethodName);
                    if (method == null)
                        throw new MessageException(MessageError.MethodNotFound);

                    if (method.Parameters.Count != (request.Arguments?.Count ?? 0))
                        throw new MessageException(MessageError.ArgumentCountMismatch);

                    var parameters = method.Parameters.Select((x, i) => x.Type.Parse(ObjectContext, request.Arguments?[i])).ToArray();
                    var task = (Task)method.MethodInfo.Invoke(obj, parameters);
                    await task;

                    if (method.ResultType != null)
                        response.Result = method.ResultType.Serialize(ObjectContext, ((dynamic)task).Result);

                    response.Error = MessageError.NoError;
                    response.MessageType = MessageType.CallResponse;
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

            return JsonConvert.SerializeObject(responseObj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}