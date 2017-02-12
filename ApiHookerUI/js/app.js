define(["require", "exports", "knockout", "./WebSocketHandler", "./RemoteModel/UIApi", "./JsonRpc", "knockout-es5"], function (require, exports, ko, WebSocketHandler_1, UIApi_1, JsonRpc_1) {
    "use strict";
    var rpc = new JsonRpc_1.JsonRpc(null);
    var uiApi = new UIApi_1.default(rpc, "api");
    ko.track(uiApi);
    ko.applyBindings(uiApi, document.getElementById("uiApi"));
    var ws = new WebSocketHandler_1.default("ws://127.0.0.1:1338/");
    ws.onConnected = socket => {
        rpc.setSocket(socket);
        rpc.refreshObject(uiApi).then(() => {
            console.log('loaded', uiApi);
            uiApi.launchAndInject("TestApp.exe").then(p => {
                console.log('launch process', p);
            });
        });
        //var api = new UIApi(rpc, "api");
        //rpc.refreshObject(api).then(() => {
        //    console.log('api.hookableMethods', api.hookableMethods);
        //});
        //var uiApi = rpc.createFrom(UIApi, {
        //    ResourceId: "api",
        //    HookableMethods: [
        //        { ResourceId: "hookableMethod/Method1", Name: "Method1", hookIt: false },
        //        { ResourceId: "hookableMethod/Method2", Name: "Method2", hookIt: true },
        //    ]
        //});
        //console.log(uiApi);
        //api.echo("hello").then(response => console.log(`echo response: ${response}`));
        //api.getHookableMethods().then(x => console.log(`getHookableMethods`, x));
    };
    ws.start();
    class KoTest {
        constructor(firstName, lastName) {
            this.firstName = firstName;
            this.lastName = lastName;
            ko.track(this);
        }
        get fullName() { return `${this.firstName} ${this.lastName}`; }
    }
    class AppModel {
    }
    var koTest = new KoTest('first', 'last');
    ko.applyBindings(koTest, document.getElementById("testModel"));
});
//# sourceMappingURL=app.js.map