declare var socket: WebSocket;
window.onload = () => {
    function startWebSocket() {
        socket = new WebSocket("ws://127.0.0.1:1338/");
        socket.onopen = openEvent => {
            console.log('websocket connected.');
            onWsConnected();
        };
        socket.onerror = errorEvent => {
            console.log('websocket error', errorEvent, socket);
            if (socket.readyState === WebSocket.CLOSED) return;
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
    static createFrom<T>(type: { new (): T; }, source: any): T {
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
    public messageType: "Error" | "Call" | "CallResponse";
    public error: "UnexpectedError" | "NoError" | "UnknownMessageType" | "ResourceNotFound" | "MethodNotFound" | "ArgumentCountMismatch" | "UnknownArgumentType" | "NotAllowedOrigin";
    public messageId: string;
    public resourceId: string;
    public methodName: string;
    public arguments: any[];
    public value: any;

    public serialize(): string {
        return JSON.stringify(this);
    }

    public static parse(source: any) {
        return ObjectHelper.createFrom(RpcMessage, JSON.parse(source));
    }
}

class JsonRpc {
    nextMessageId = 0;
    msgHandlers: { [msgId: string]: { resolve, reject } } = { };

    constructor(public socket: WebSocket) {
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

    call(resourceId: string, methodName: string, args: any[]): Promise<any> {
        return new Promise<any>((resolve, reject) => {
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
};

class UIObject {
    constructor(public jsonRpc: JsonRpc, public resourceId: string){ }
};

class UIHookableMethod {
    public static createFrom(source: any) {
        return ObjectHelper.createFrom(UIHookableMethod, source);
    }
}

class UIApi extends UIObject {
    echo(message: string): Promise<string> {
        return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => <string>x);
    }

    getHookableMethods(): Promise<UIHookableMethod[]> {
        return this.jsonRpc.call(this.resourceId, "GetHookableMethods", []).
            then(items => items.map(x => UIHookableMethod.createFrom(x)));
    }
};