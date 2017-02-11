using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiveObjects.Communication;
using LiveObjects.DependencyInjection;
using LiveObjects.Logging;
using LiveObjects.Utils;
using LiveObjects.Utils.ExtensionMethods;
using Newtonsoft.Json;

namespace LiveObjects.Test
{
    public static class Tests
    {
        public static string TestCaseFolder => AppDomain.CurrentDomain.BaseDirectory.Split(@"LiveObjects\", 2)[0] + @"LiveObjects\Test\TestCases\";

        public static string Assert(string testName, string value)
        {
            var checkFn = $@"{TestCaseFolder}\{testName}";
            if (File.Exists(checkFn))
            {
                var good = File.ReadAllText(checkFn);
                if (value != good)
                    throw new Exception("Test failed!");
            }
            else
                File.WriteAllText(FileUtils.ProvidePath(checkFn), value);

            return value;
        }

        public static string Assert(string testName, object value)
        {
            var result = JsonConvert.SerializeObject(value, Formatting.Indented);
            return Assert($"{testName}.json", result);
        }

        public static string RunTest(MessageBridge bridge, string testName, Message request)
        {
            var result = JsonConvert.DeserializeObject(bridge.ProcessMessageAsync(JsonConvert.SerializeObject(request)).Result);
            return Assert($"test_{testName}", result);
        }

        public static string ExpectChange(MessageBridge bridge, string testName, Action localAction)
        {
            var changes = new List<Message>();

            ChangeMessageEvent chEvent = (_, changeMsg) => changes.Add(changeMsg);
            bridge.ChangeMessageEvent += chEvent;
            localAction();
            bridge.ChangeMessageEvent -= chEvent;

            return Assert(testName, changes);
        }

        public static void RunTests()
        {
            Directory.Delete(TestCaseFolder, true);
            Dependency.Register<ILogger>(new ConsoleLogger());

            var testObj = new TestObject();

            var bridge = new MessageBridge();
            bridge.ObjectContext.PublishObject(testObj);

            Assert("model_testObject", bridge.ObjectContext.TypeContext.TypeDescriptors.Select(x => x.Value));

            var to1 = RunTest(bridge, "to1", new Message { MessageType = MessageType.Call, ResourceId = "testObject", MethodName = "echo", Arguments = new List<object> { "hello" } });
            var to2 = RunTest(bridge, "to2", new Message { MessageType = MessageType.Call, ResourceId = "testObject", MethodName = "slowEcho", Arguments = new List<object> { "hello" } });
            var to3 = RunTest(bridge, "to3", new Message { MessageType = MessageType.Get, ResourceId = "testObject" });
            var to4 = ExpectChange(bridge, "to4", () => testObj.StringProperty = "new StringProperty value");

            //var t1 = test("t1", new { messageType = "call", resourceId = "api", methodName = "echo", arguments = new[] { "data" } });
            //var t2 = test("t2", new { messageType = "call", resourceId = "api", methodName = "LaunchAndInject", arguments = new[] { "path" } });
            //var t3 = test("t3", new { messageType = "call", resourceId = "process/path", methodName = "ResumeMainThread" });
            //var t4 = test("t4", new { messageType = "call", resourceId = "api", methodName = "GetHookableMethods" });
            //var t5 = test("t5", new { messageType = "call", resourceId = "process/path", methodName = "HookMethods", arguments = new[] { new[] { new { ResourceId = "hookableMethod/GetConsoleTitleA" } } } });
        }
    }
}