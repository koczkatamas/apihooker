using LiveObjects.ModelDescription;

namespace LiveObjects.ObjectContext
{
    public interface IObjectRepository
    {
        IUIObject GetObject(string resourceId);
        void PublishObject(IUIObject obj);
    }
}