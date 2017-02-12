define(["require", "exports", "./WebSocketHandler", "./UIApi", "./JsonRpc"], function (require, exports, WebSocketHandler_1, UIApi_1, JsonRpc_1) {
    "use strict";
    var ws = new WebSocketHandler_1.default("ws://127.0.0.1:1338/");
    ws.onConnected = socket => {
        var rpc = new JsonRpc_1.default(socket);
        var api = new UIApi_1.default(rpc, "api");
        //api.echo("hello").then(response => console.log(`echo response: ${response}`));
        api.getHookableMethods().then(x => console.log(`getHookableMethods`, x));
    };
    ws.start();
});
//# sourceMappingURL=app.js.map