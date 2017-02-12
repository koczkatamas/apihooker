define(["require", "exports", "knockout", "./WebSocketHandler", "./RemoteModel/UIApi", "./JsonRpc", "knockout-es5"], function (require, exports, ko, WebSocketHandler_1, UIApi_1, JsonRpc_1) {
    "use strict";
    var rpc = new JsonRpc_1.JsonRpc(null);
    var uiApi = new UIApi_1.default(rpc, "api");
    ko.track(uiApi);
    ko.applyBindings(uiApi, document.getElementById("uiApi"));
    var isFirstLaunch = true;
    var ws = new WebSocketHandler_1.default("ws://127.0.0.1:1338/");
    ws.onConnected = socket => {
        rpc.setSocket(socket);
        rpc.refreshObject(uiApi).then(() => {
            console.log('loaded', uiApi);
            if (!isFirstLaunch)
                return;
            isFirstLaunch = false;
            uiApi.launchAndHook("TestApp.exe").then(p => {
                console.log('launched process', p);
                setTimeout(() => {
                    p.readNewCallRecords().then(newCrs => {
                        console.log('new call records', newCrs);
                    }).then(() => {
                        return p.unhookAndWaitForExit();
                    }).then(() => {
                        console.log('unhooked');
                    });
                }, 500);
            });
        });
    };
    ws.start();
});
//# sourceMappingURL=app.js.map