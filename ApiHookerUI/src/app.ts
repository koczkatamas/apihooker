/// <reference path="../Scripts/typings/knockout.es5/knockout.es5.d.ts"/>
import * as ko from "knockout";
import "knockout-es5";

import WebSocketHandler from "./WebSocketHandler";
import UIApi from "./RemoteModel/UIApi";
import { JsonRpc, UIObject } from "./JsonRpc";

var rpc = new JsonRpc(null);
var uiApi = new UIApi(rpc, "api");
ko.track(uiApi);
ko.applyBindings(uiApi, document.getElementById("uiApi"));

var isFirstLaunch = true;
var ws = new WebSocketHandler("ws://127.0.0.1:1338/");
ws.onConnected = socket => {
    rpc.setSocket(socket);
    rpc.refreshObject(uiApi).then(() => {
        console.log('loaded', uiApi);
        if (!isFirstLaunch) return;
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
}
ws.start();
