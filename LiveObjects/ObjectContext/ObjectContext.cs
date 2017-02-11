using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using LiveObjects.Communication;
using LiveObjects.DependencyInjection;
using LiveObjects.Logging;
using LiveObjects.ModelDescription;
using LiveObjects.Utils.ExtensionMethods;

namespace LiveObjects.ObjectContext
{
    public delegate void LiveObjectPropertyChangedEvent(ObjectContext objectContext, ILiveObject obj, string propertyName);
    public delegate void ListChangedEvent(ObjectContext objectContext, ILiveObject obj, string propertyName, NotifyCollectionChangedEventArgs eventArgs);

    public class ObjectContext : IObjectContext
    {
        public ILogger Logger = Dependency.Get<ILogger>(false);
        public TypeContext TypeContext { get; protected set; } = TypeContext.Instance;
        public ConcurrentDictionary<string, ILiveObject> ObjectRepository { get; protected set; } = new ConcurrentDictionary<string, ILiveObject>(StringComparer.OrdinalIgnoreCase);

        public bool TrackChanges { get; set; } = true;
        public event LiveObjectPropertyChangedEvent ObjectPropertyChanged;
        public event ListChangedEvent ListChanged;

        public void PublishObject(ILiveObject obj)
        {
            var typeDesc = (ObjectDescriptor) TypeContext.GetTypeDescriptor(obj.GetType());

            var objId = obj.ResourceId;
            var existingObj = ObjectRepository.GetValueOrDefault(objId);
            //if (existingObj != null && existingObj != obj)
            //    throw new Exception($"Object's key is not unique: {objId}");
            if (existingObj != null && existingObj != obj)
                Logger.Log($"[Warning] Object's key is not unique: {objId}");
            ObjectRepository[objId] = obj;

            if (TrackChanges)
            {
                if (obj is INotifyPropertyChanged)
                    ((INotifyPropertyChanged) obj).PropertyChanged += (sender, args) => ObjectPropertyChanged?.Invoke(this, obj, args.PropertyName);

                foreach (var propDesc in typeDesc.Properties.Values.Where(x => typeof(INotifyCollectionChanged).IsAssignableFrom(x.PropertyInfo.PropertyType)))
                {
                    var propValue = (INotifyCollectionChanged) propDesc.PropertyInfo.GetValue(obj);
                    if (propValue == null) continue;

                    propValue.CollectionChanged += (sender, args) => ListChanged?.Invoke(this, obj, propDesc.PropertyInfo.Name, args);
                }
            }
        }

        public ILiveObject GetObject(string resourceId) => ObjectRepository.GetValueOrDefault(resourceId ?? "");
    }
}