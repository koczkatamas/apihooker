using System;
using Newtonsoft.Json;

namespace ApiHooker.UiApi.JsonRpc
{
    public static class JsonRpcTest
    {
        public static void TestRpc()
        {
            var uiApi = new UIApi();
            var jsonRpc = new JsonRpc();
            jsonRpc.PublishObject(uiApi);

            Func<object, string> test = req => JsonConvert.SerializeObject(JsonConvert.DeserializeObject(jsonRpc.ProcessMessageAsync(JsonConvert.SerializeObject(req)).Result), Formatting.Indented);

            var t1 = test(new { messageType = "call", resourceId = "api", methodName = "echo", arguments = new[] { "data" } });
            var t2 = test(new { messageType = "call", resourceId = "api", methodName = "LaunchAndInject", arguments = new[] { "path" } });
            var t3 = test(new { messageType = "call", resourceId = "process/path", methodName = "ResumeMainThread" });
            var t4 = test(new { messageType = "call", resourceId = "api", methodName = "GetHookableMethods" });
            var t5 = test(new { messageType = "call", resourceId = "process/path", methodName = "HookMethods", arguments = new[] { new[] { new { ResourceId = "hookableMethod/GetConsoleTitleA" } } } });
        }
    }
}