using System;

namespace ApiHooker.UiApi.JsonRpc
{
    public class RpcMessageException : Exception
    {
        public RpcMessageError Error { get; protected set; }

        public RpcMessageException(RpcMessageError error) { Error = error; }
    }
}