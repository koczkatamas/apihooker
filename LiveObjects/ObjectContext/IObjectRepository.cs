using LiveObjects.ModelDescription;

namespace LiveObjects.ObjectContext
{
    public interface IObjectRepository
    {
        ILiveObject GetObject(string resourceId);
        void PublishObject(ILiveObject obj);
    }
}