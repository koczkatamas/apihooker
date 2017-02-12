using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class UIHookableMethod : ILiveObject
    {
        [Publish]
        public string ResourceId => $"hookableMethod/{Name}";

        [Publish]
        public string Name { get; set; }
    }
}