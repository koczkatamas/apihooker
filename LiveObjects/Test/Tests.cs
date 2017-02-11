using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var result = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return Assert($"{testName}.json", result);
        }

        public static string RunTest(MessageBridge bridge, string testName, Message request)
        {
            var result = JsonConvert.DeserializeObject(bridge.ProcessMessageAsync(JsonConvert.SerializeObject(request)).Result);
            return Assert($"test_{testName}", result);
        }

        public static Message[] GetChanges(MessageBridge bridge, Action localAction)
        {
            var changes = new List<Message>();

            ChangeMessageEvent chEvent = (_, changeMsg) => changes.Add(changeMsg);
            bridge.ChangeMessageEvent += chEvent;
            localAction();
            bridge.ChangeMessageEvent -= chEvent;

            return changes.ToArray();
        }

        public static string ExpectChange(MessageBridge bridge, string testName, Action localAction)
        {
            var changes = GetChanges(bridge, localAction);
            return Assert(testName, changes);
        }

        public static string ExpectListChange(MessageBridge bridge, string testName, IEnumerable list, Action localAction)
        {
            var originalList = list.Cast<object>().ToList();
            var changes = GetChanges(bridge, localAction);
            var newList = list.Cast<object>().ToArray();

            // simulate changes...
            var shadowList = originalList.ToList();
            foreach (var change in changes.Where(x => x.ListChanges != null).SelectMany(x => x.ListChanges))
            {
                if(change.Action == ListChangeAction.Add)
                    shadowList.Insert(change.Index, change.Value);
                else if (change.Action == ListChangeAction.Remove)
                {
                    if(shadowList[change.Index] != change.Value)
                        throw new Exception("Removed item does not match!");

                    shadowList.RemoveAt(change.Index);
                }
            }

            if(!newList.SequenceEqual(shadowList))
                throw new Exception("List change events do not match!");

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

            var newValue = "successful StringProperty change";
            string to5a = null;
            var to5b = ExpectChange(bridge, "to5b", () => to5a = RunTest(bridge, "to5a", 
                new Message { MessageType = MessageType.SetProperty, ResourceId = "testObject", PropertyName = "StringProperty", Value = newValue }));
            if(testObj.StringProperty != newValue)
                throw new Exception("Could not change property value!");

            // Action = Remove, NewStartingIndex = -1, OldStartingIndex = 0, OldItems = ["ListItem #1"]
            var to6 = ExpectListChange(bridge, "to6", testObj.List, () => testObj.List.RemoveAt(0));
            // Action = Add, NewStartingIndex = 0, OldStartingIndex = -1, NewItems = ["inserted item #0"]
            var to7 = ExpectListChange(bridge, "to7", testObj.List, () => testObj.List.Insert(0, "inserted item #0"));
            // Action = Add, NewStartingIndex = 5, OldStartingIndex = -1, NewItems = ["added item"]
            var to8 = ExpectListChange(bridge, "to8", testObj.List, () => testObj.List.Add("added item"));

            // Action = Move, NewStartingIndex = 1, OldStartingIndex = 0, NewItems = ["inserted item #0"], OldItems = ["inserted item #0"]
            var to9 = ExpectListChange(bridge, "to9", testObj.List, () => testObj.List.Move(0, 1));
            var to10 = ExpectListChange(bridge, "to10", testObj.List, () => testObj.List.Move(2, 0));
            
            // Action = Replace, NewStartingIndex = 0, OldStartingIndex = 0, NewItems = ["new value"], OldItems = ["ListItem #2"]
            var to11 = ExpectListChange(bridge, "to11", testObj.List, () => testObj.List[0] = "new value");

            var newListIem = "Pushed Item #0";
            var to12 = RunTest(bridge, "to12", new Message { MessageType = MessageType.ChangeList, ResourceId = "testObject", PropertyName = "List", ListChanges = new List<ListChangeItem> {
                new ListChangeItem { Action = ListChangeAction.Add, Index = 0, Value = newListIem } } });

            if (testObj.List[0] != newListIem)
                throw new Exception("Could not add list item!");

            var to13 = RunTest(bridge, "to13", new Message { MessageType = MessageType.ChangeList, ResourceId = "testObject", PropertyName = "List", ListChanges = new List<ListChangeItem> {
                new ListChangeItem { Action = ListChangeAction.Remove, Index = 0, Value = newListIem } } });

            if (testObj.List[0] == newListIem)
                throw new Exception("Could not remove list item!");

            //var to7 = RunTest(bridge, "to7", new Message { MessageType = MessageType.ChangeList, ResourceId = "testObject", PropertyName = "List", ListChangeData = new ListChangeData()
            //{
            //    Action = ListChangeAction.Add,
            //    NewItems = 
            //} });

            //var t1 = test("t1", new { messageType = "call", resourceId = "api", methodName = "echo", arguments = new[] { "data" } });
            //var t2 = test("t2", new { messageType = "call", resourceId = "api", methodName = "LaunchAndInject", arguments = new[] { "path" } });
            //var t3 = test("t3", new { messageType = "call", resourceId = "process/path", methodName = "ResumeMainThread" });
            //var t4 = test("t4", new { messageType = "call", resourceId = "api", methodName = "GetHookableMethods" });
            //var t5 = test("t5", new { messageType = "call", resourceId = "process/path", methodName = "HookMethods", arguments = new[] { new[] { new { ResourceId = "hookableMethod/GetConsoleTitleA" } } } });
        }
    }
}