using System;

namespace LiveObjects.ModelDescription
{
    public class PublicationPolicyAttribute : Attribute
    {
        public DefaultPublicationMode DefaultPublicationMode { get; protected set; }

        public PublicationPolicyAttribute(DefaultPublicationMode defaultPublicationMode)
        {
            DefaultPublicationMode = defaultPublicationMode;
        }
    }
}