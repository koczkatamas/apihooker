window.onload = () => {
    function startWebSocket() {
        socket = new WebSocket("ws://127.0.0.1:1338/");
        socket.onopen = openEvent => {
            console.log('websocket connected.');
            onWsConnected();
        };
        socket.onerror = errorEvent => {
            console.log('websocket error', errorEvent, socket);
            if (socket.readyState === WebSocket.CLOSED)
                return;
        };
        socket.onclose = closeEvent => {
            console.log('websocket auto reconnecting...', closeEvent);
            setTimeout(startWebSocket, 500);
        };
    }
    startWebSocket();
    function onWsConnected() {
        var rpc = new JsonRpc(socket);
        var api = new UIApi(rpc, "api");
        //api.echo("hello").then(response => console.log(`echo response: ${response}`));
        api.getHookableMethods().then(x => console.log(`getHookableMethods`, x));
        //socket.send(JSON.stringify({ messageType: "call", resourceId: "api", methodName: "GetHookableMethods" }));
    }
};
class ObjectHelper {
    static createFrom(type, source) {
        var result = new type();
        this.copy(source, result);
        return result;
    }
    static copy(src, dst) {
        for (var srcKey of Object.getOwnPropertyNames(src)) {
            var dstKey = srcKey[0].toLowerCase() + srcKey.substr(1);
            dst[dstKey] = src[srcKey];
        }
    }
}
class RpcMessage {
    serialize() {
        return JSON.stringify(this);
    }
    static parse(source) {
        return ObjectHelper.createFrom(RpcMessage, JSON.parse(source));
    }
}
class JsonRpc {
    constructor(socket) {
        this.socket = socket;
        this.nextMessageId = 0;
        this.msgHandlers = {};
        socket.onmessage = event => {
            var responseMsg = RpcMessage.parse(event.data);
            console.log('websocket msg', responseMsg);
            if (responseMsg.messageType === "CallResponse") {
                var msgHandler = this.msgHandlers[responseMsg.messageId];
                console.log('msgHandler', msgHandler);
                if (responseMsg.error === "NoError")
                    msgHandler.resolve(responseMsg.value);
                else
                    msgHandler.reject(responseMsg.error);
            }
        };
    }
    call(resourceId, methodName, args) {
        return new Promise((resolve, reject) => {
            var msg = new RpcMessage();
            msg.messageType = "Call";
            msg.messageId = `msg_${this.nextMessageId++}`;
            msg.resourceId = resourceId;
            msg.methodName = methodName;
            msg.arguments = args;
            this.msgHandlers[msg.messageId] = { resolve, reject };
            var msgJson = msg.serialize();
            console.log('call', arguments, msgJson);
            this.socket.send(msgJson);
        });
    }
}
;
class UIObject {
    constructor(jsonRpc, resourceId) {
        this.jsonRpc = jsonRpc;
        this.resourceId = resourceId;
    }
}
;
class UIHookableMethod {
    static createFrom(source) {
        return ObjectHelper.createFrom(UIHookableMethod, source);
    }
}
class UIApi extends UIObject {
    echo(message) {
        return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => x);
    }
    getHookableMethods() {
        return this.jsonRpc.call(this.resourceId, "GetHookableMethods", []).
            then(items => items.map(x => UIHookableMethod.createFrom(x)));
    }
}
;
//# sourceMappingURL=app.js.map