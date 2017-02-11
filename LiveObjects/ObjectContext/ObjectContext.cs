using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using LiveObjects.DependencyInjection;
using LiveObjects.Logging;
using LiveObjects.ModelDescription;
using LiveObjects.Utils.ExtensionMethods;

namespace LiveObjects.ObjectContext
{
    public delegate void LiveObjectPropertyChangedEvent(ObjectContext objectContext, ILiveObject obj, string propertyName);

    public class ObjectContext : IObjectContext
    {
        public ILogger Logger = Dependency.Get<ILogger>(false);
        public TypeContext TypeContext { get; protected set; } = TypeContext.Instance;
        public ConcurrentDictionary<string, ILiveObject> ObjectRepository { get; protected set; } = new ConcurrentDictionary<string, ILiveObject>(StringComparer.OrdinalIgnoreCase);

        public bool TrackChanges { get; set; } = true;
        public event LiveObjectPropertyChangedEvent ObjectPropertyChanged;

        public void PublishObject(ILiveObject obj)
        {
            TypeContext.GetTypeDescriptor(obj.GetType());

            var objId = obj.ResourceId;
            var existingObj = ObjectRepository.GetValueOrDefault(objId);
            //if (existingObj != null && existingObj != obj)
            //    throw new Exception($"Object's key is not unique: {objId}");
            if (existingObj != null && existingObj != obj)
                Logger.Log($"[Warning] Object's key is not unique: {objId}");
            ObjectRepository[objId] = obj;

            if (TrackChanges && obj is INotifyPropertyChanged)
                ((INotifyPropertyChanged)obj).PropertyChanged += (sender, args) => ObjectPropertyChanged?.Invoke(this, (ILiveObject) sender, args.PropertyName);
        }

        public ILiveObject GetObject(string resourceId) => ObjectRepository.GetValueOrDefault(resourceId ?? "");
    }
}