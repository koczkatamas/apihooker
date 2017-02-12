using ApiHooker.Model;

namespace ApiHooker.UiApi
{
    public class HookedMethod
    {
        public ApiMethod ApiMethod { get; set; }
        public bool SaveCallback { get; set; }

        public HookedMethod(ApiMethod apiMethod)
        {
            ApiMethod = apiMethod;
        }
    }
}