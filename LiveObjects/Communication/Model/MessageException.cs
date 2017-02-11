using System;

namespace LiveObjects.Communication
{
    public class MessageException : Exception
    {
        public MessageError Error { get; protected set; }

        public MessageException(MessageError error) { Error = error; }
    }
}