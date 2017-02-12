define(["require", "exports", "knockout", "./WebSocketHandler", "./UIApi", "./JsonRpc", "knockout-es5"], function (require, exports, ko, WebSocketHandler_1, UIApi_1, JsonRpc_1) {
    "use strict";
    var ws = new WebSocketHandler_1.default("ws://127.0.0.1:1338/");
    ws.onConnected = socket => {
        var rpc = new JsonRpc_1.default(socket);
        var api = new UIApi_1.default(rpc, "api");
        //api.echo("hello").then(response => console.log(`echo response: ${response}`));
        api.getHookableMethods().then(x => console.log(`getHookableMethods`, x));
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
    var koTest = new KoTest('first', 'last');
    ko.applyBindings(koTest);
});
//# sourceMappingURL=app.js.map