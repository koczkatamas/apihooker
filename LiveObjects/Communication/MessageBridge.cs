using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
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
        public bool AllowConcurrentCalls { get; set; }
        private readonly PropertyChangeSilencer silencedProps = new PropertyChangeSilencer();

        public MessageBridge()
        {
            ObjectContext.ObjectPropertyChanged += (objContext, liveObj, propName) =>
            {
                if (silencedProps.IsSilenced(liveObj, propName)) return;

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

            ObjectContext.ListChanged += (objContext, liveObj, propDesc, changeArgs) =>
            {
                var propName = propDesc.PropertyInfo.Name;
                if (silencedProps.IsSilenced(liveObj, propName)) return;

                var isReset = changeArgs.Action == NotifyCollectionChangedAction.Reset;
                var changeMsg = new Message
                {
                    Error = MessageError.NoError,
                    MessageType = isReset ? MessageType.PropertyChanged : MessageType.ListChanged,
                    ResourceId = liveObj.ResourceId,
                    PropertyName = propName,
                    ListChanges = isReset ? null : new List<ListChangeItem>()
                };

                // Action = Add,     NewStartingIndex =  0, OldStartingIndex = -1, NewItems = ["inserted item #0"]
                // Action = Add,     NewStartingIndex =  5, OldStartingIndex = -1, NewItems = ["added item"]
                // Action = Remove,  NewStartingIndex = -1, OldStartingIndex =  0, NewItems = null,                 OldItems = ["ListItem #1"]
                // Action = Move,    NewStartingIndex =  1, OldStartingIndex =  0, NewItems = ["inserted item #0"], OldItems = ["inserted item #0"]
                // Action = Replace, NewStartingIndex =  0, OldStartingIndex =  0, NewItems = ["new value"],        OldItems = ["ListItem #2"]
                for (var i = 0; i < changeArgs.OldItems?.Count; i++)
                    changeMsg.ListChanges.Add(new ListChangeItem { Action = ListChangeAction.Remove, Index = changeArgs.OldStartingIndex, Value = changeArgs.OldItems[i] });

                for(var i = 0; i < changeArgs.NewItems?.Count; i++)
                    changeMsg.ListChanges.Add(new ListChangeItem { Action = ListChangeAction.Add, Index = changeArgs.NewStartingIndex + i, Value = changeArgs.NewItems[i] });

                ChangeMessageEvent?.Invoke(this, changeMsg);
            };
        }

        private readonly SemaphoreSlim processMessageLock = new SemaphoreSlim(1);

        public async Task<Message> ProcessMessageAsync(Message request)
        {
            var response = new Message { Error = MessageError.UnexpectedError, MessageId = request.MessageId };

            var locked = !AllowConcurrentCalls;
            if (locked)
                await processMessageLock.WaitAsync();

            try
            {
                if (String.IsNullOrEmpty(request.ResourceId))
                    throw new MessageException(MessageError.ResourceIdMissing);

                var obj = ObjectContext.GetObject(request.ResourceId);
                if (obj == null)
                    throw new MessageException(MessageError.ResourceNotFound);

                var typeInfo = (ObjectDescriptor) ObjectContext.TypeContext.GetTypeDescriptor(obj.GetType());

                Func<PropertyDescriptor> getPropDesc = () =>
                {
                    if (String.IsNullOrEmpty(request.PropertyName))
                        throw new MessageException(MessageError.PropertyNameMissing);

                    var propDesc = typeInfo.Properties.GetValueOrDefault(request.PropertyName);
                    if (propDesc == null)
                        throw new MessageException(MessageError.PropertyNotFound);

                    return propDesc;
                };

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
                    var propDesc = getPropDesc();
                    using (silencedProps.SilenceThis(obj, propDesc))
                        propDesc.PropertyInfo.SetValue(obj, request.Value);

                    response.Error = MessageError.NoError;
                    response.MessageType = MessageType.SuccessConfirmation;
                }
                else if (request.MessageType == MessageType.ChangeList)
                {
                    var propDesc = getPropDesc();
                    var list = propDesc.PropertyInfo.GetValue(obj) as IList;
                    if (list == null)
                        throw new MessageException(MessageError.ListDesynchronized);

                    using (silencedProps.SilenceThis(obj, propDesc))
                    {
                        foreach (var change in request.ListChanges)
                        {
                            if (change.Action == ListChangeAction.Add)
                                list.Insert(change.Index, change.Value);
                            else if (change.Action == ListChangeAction.Remove)
                            {
                                if (!(0 <= change.Index && change.Index < list.Count) || (change.Value != null && !Equals(list[change.Index], change.Value)))
                                    throw new MessageException(MessageError.ListDesynchronized);

                                list.RemoveAt(change.Index);
                            }
                        }
                    }

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
            finally
            {
                if (locked)
                    processMessageLock.Release();
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