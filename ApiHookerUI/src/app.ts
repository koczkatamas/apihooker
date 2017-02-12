/// <reference path="../Scripts/typings/knockout.es5/knockout.es5.d.ts"/>
import * as ko from "knockout";

import WebSocketHandler from "./WebSocketHandler";
import UIApi from "./UIApi";
import JsonRpc from "./JsonRpc";

var ws = new WebSocketHandler("ws://127.0.0.1:1338/");
ws.onConnected = socket => {
    var rpc = new JsonRpc(socket);
    var api = new UIApi(rpc, "api");
    //api.echo("hello").then(response => console.log(`echo response: ${response}`));
    api.getHookableMethods().then(x => console.log(`getHookableMethods`, x));
}
ws.start();