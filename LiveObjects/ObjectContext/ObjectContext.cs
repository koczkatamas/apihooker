using System;
using System.Collections.Concurrent;
using LiveObjects.DependencyInjection;
using LiveObjects.Logging;
using LiveObjects.ModelDescription;
using LiveObjects.Utils.ExtensionMethods;

namespace LiveObjects.ObjectContext
{
    public class ObjectContext : IObjectContext
    {
        public ILogger Logger = Dependency.Get<ILogger>(false);
        public TypeContext TypeContext { get; protected set; } = TypeContext.Instance;
        public ConcurrentDictionary<string, IUIObject> ObjectRepository { get; protected set; } = new ConcurrentDictionary<string, IUIObject>(StringComparer.OrdinalIgnoreCase);

        public void PublishObject(IUIObject obj)
        {
            var objId = obj.ResourceId;
            var existingObj = ObjectRepository.GetValueOrDefault(objId);
            //if (existingObj != null && existingObj != obj)
            //    throw new Exception($"Object's key is not unique: {objId}");
            if (existingObj != null && existingObj != obj)
                Logger.Log($"[Warning] Object's key is not unique: {objId}");
            ObjectRepository[objId] = obj;

            TypeContext.GetTypeDescriptor(obj.GetType());
        }

        public IUIObject GetObject(string resourceId) => ObjectRepository.GetValueOrDefault(resourceId ?? "");
    }
}