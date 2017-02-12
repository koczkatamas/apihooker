using ApiHooker.Model;
using LiveObjects.ModelDescription;
using Newtonsoft.Json;

namespace ApiHooker.UiApi
{
    public class UIHookableMethod : ILiveObject
    {
        public ApiMethod ApiMethod { get; protected set; }

        [Publish]
        public string ResourceId => $"hookableMethod/{Name}";

        [Publish]
        public string Name => ApiMethod.MethodName;

        [Publish]
        public bool HookIt { get; set; }

        public UIHookableMethod(ApiMethod apiMethod)
        {
            ApiMethod = apiMethod;
        }
    }
}