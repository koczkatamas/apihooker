using System;
using System.IO;
using ApiHooker.Utils;
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

            Func<string, object, string> test = (name, req) =>
            {
                var result = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(jsonRpc.ProcessMessageAsync(JsonConvert.SerializeObject(req)).Result), Formatting.Indented);

                var checkFn = $@"..\..\WindowsController\UiApi\JsonRpc\TestCases\test_{name}.json";
                if (File.Exists(checkFn))
                {
                    var good = File.ReadAllText(checkFn);
                    if (result != good)
                        throw new Exception("Test failed!");
                }
                else
                    File.WriteAllText(FileUtils.ProvidePath(checkFn), result);

                return result;
            };

            var t1 = test("t1", new { messageType = "call", resourceId = "api", methodName = "echo", arguments = new[] { "data" } });
            var t2 = test("t2", new { messageType = "call", resourceId = "api", methodName = "LaunchAndInject", arguments = new[] { "path" } });
            var t3 = test("t3", new { messageType = "call", resourceId = "process/path", methodName = "ResumeMainThread" });
            var t4 = test("t4", new { messageType = "call", resourceId = "api", methodName = "GetHookableMethods" });
            var t5 = test("t5", new { messageType = "call", resourceId = "process/path", methodName = "HookMethods", arguments = new[] { new[] { new { ResourceId = "hookableMethod/GetConsoleTitleA" } } } });
        }
    }
}