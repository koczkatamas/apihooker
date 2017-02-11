namespace LiveObjects.Communication
{
    public enum MessageError
    {
        UnexpectedError,
        NoError,
        RequestParsingError,
        UnknownMessageType,
        ResourceNotFound,
        ResourceIdMissing,
        PropertyNameMissing,
        MethodNotFound,
        PropertyNotFound,
        ArgumentCountMismatch,
        UnknownArgumentType,
        NotAllowedOrigin,
        ListDesynchronized,
    }
}