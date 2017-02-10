namespace ApiHooker.UiApi.JsonRpc
{
    public interface IObjectRepository
    {
        IUIObject GetObject(string resourceId);
        void PublishObject(IUIObject obj);
    }
}