using System;
using System.Collections.Generic;
using System.Linq;
using LiveObjects.ModelDescription;

namespace LiveObjects.Communication
{
    public class PropertyChangeSilencer
    {
        protected readonly List<SilencedProperty> Properties = new List<SilencedProperty>();

        protected class SilencedProperty : IDisposable
        {
            public readonly PropertyChangeSilencer Silencer;
            public ILiveObject Object;
            public string PropertyName;

            public SilencedProperty(PropertyChangeSilencer silencer, ILiveObject obj, string propertyName)
            {
                Silencer = silencer;
                Object = obj;
                PropertyName = propertyName;

                lock (silencer)
                    silencer.Properties.Add(this);
            }

            public void Dispose()
            {
                lock(Silencer)
                    Silencer.Properties.Remove(this);
            }
        }

        public bool IsSilenced(ILiveObject obj, string propName)
        {
            lock(this)
                return Properties.Any(x => x.Object == obj && x.PropertyName == propName);
        }

        public IDisposable SilenceThis(ILiveObject obj, PropertyDescriptor propDesc)
        {
            return new SilencedProperty(this, obj, propDesc.PropertyInfo.Name);
        }
    }
}