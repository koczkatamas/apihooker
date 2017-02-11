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
using PropertyDescriptor = LiveObjects.ModelDescription.PropertyDescriptor;

namespace LiveObjects.ObjectContext
{
    public delegate void LiveObjectPropertyChangedEvent(ObjectContext objectContext, ILiveObject obj, string propertyName);
    public delegate void ListChangedEvent(ObjectContext objectContext, ILiveObject obj, PropertyDescriptor propDesc, NotifyCollectionChangedEventArgs eventArgs);

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
            if(String.IsNullOrEmpty(objId))
                throw new Exception("Cannot publish a LiveObject with missing ResourceId!");

            var existingObj = ObjectRepository.GetValueOrDefault(objId);
            //if (existingObj != null && existingObj != obj)
            //    throw new Exception($"Object's key is not unique: {objId}");
            if (existingObj != null && existingObj != obj)
                Logger.Log($"[Warning] Object's key is not unique: {objId}");
            ObjectRepository[objId] = obj;

            if (TrackChanges)
            {
                if (obj is INotifyPropertyChanged)
                    ((INotifyPropertyChanged)obj).PropertyChanged += OnObjectPropertyChanged;

                foreach (var propDesc in typeDesc.Properties.Values.Where(x => typeof(INotifyCollectionChanged).IsAssignableFrom(x.PropertyInfo.PropertyType)))
                {
                    var propValue = (INotifyCollectionChanged) propDesc.PropertyInfo.GetValue(obj);
                    if (propValue == null) continue;

                    propValue.CollectionChanged += (sender, args) => ListChanged?.Invoke(this, obj, propDesc, args);
                }
            }
        }

        private void OnObjectPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            ObjectPropertyChanged?.Invoke(this, (ILiveObject)sender, args.PropertyName);
        }

        public ILiveObject GetObject(string resourceId) => ObjectRepository.GetValueOrDefault(resourceId ?? "");
    }
}