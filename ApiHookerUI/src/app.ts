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

var ws = new WebSocketHandler("ws://127.0.0.1:1338/");
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
}
ws.start();

class KoTest {
    get fullName() { return `${this.firstName} ${this.lastName}`; }

    constructor(public firstName: string, public lastName: string) {
        ko.track(this);
    }
}

class AppModel {
    
}

var koTest = new KoTest('first', 'last');
ko.applyBindings(koTest, document.getElementById("testModel"));