import { ObjectHelper } from "./ObjectHelper";
import * as ko from "knockout";

type MessageType = "Error" | "Call" | "CallResponse" | "Get" | "SetProperty";
type MessageError = "UnexpectedError" | "NoError" | "UnknownMessageType" | "ResourceNotFound" | "MethodNotFound" | "ArgumentCountMismatch" | "UnknownArgumentType" | "NotAllowedOrigin";
type UIObjectConstructor<T> = { new (rpc: JsonRpc, resourceId: string): T; };

export class RpcMessage {
    public error: MessageError;
    public messageId: string;
    public methodName: string;
    public arguments: any[];
    public propertyName: string;
    public value: any;

    constructor(public messageType?: MessageType, public resourceId?: string) { }

    public serialize(): string {
        return JSON.stringify(this);
    }

    public static parse(source: any) {
        return ObjectHelper.createFrom(RpcMessage, JSON.parse(source));
    }
}

export class UIObject {
    constructor(public jsonRpc: JsonRpc = null, public resourceId: string = '') { }

    static typedArray<T>(type: UIObjectConstructor<T>): T[] {
        var result = [];
        (<any>result).type = type;
        return result;
    }
}

export class JsonRpc {
    nextMessageId = 0;
    msgHandlers: { [msgId: string]: { resolve; reject } } = { };

    constructor(public socket?: WebSocket) {
        socket && this.setSocket(socket);
    }

    setSocket(socket: WebSocket) {
        this.socket = socket;
        socket.onmessage = event => {
            var responseMsg = RpcMessage.parse(event.data);
            console.log('websocket msg', responseMsg);
            if (responseMsg.messageId && responseMsg.messageId in this.msgHandlers) {
                var msgHandler = this.msgHandlers[responseMsg.messageId];
                //console.log('msgHandler', msgHandler);
                if (responseMsg.error === "NoError")
                    msgHandler.resolve(responseMsg.value);
                else
                    msgHandler.reject(responseMsg.error);
            }
        };
    }

    getResponse(msg: RpcMessage): Promise<any> {
        return new Promise<any>((resolve, reject) => {
            msg.messageId = msg.messageId || `msg_${this.nextMessageId++}`;
            this.msgHandlers[msg.messageId] = { resolve, reject };
            console.log('[RPC] send', msg);
            var msgJson = msg.serialize();
            this.socket.send(msgJson);
        });
    }

    call(resourceId: string, methodName: string, args: any[]): Promise<any> {
        var msg = new RpcMessage("Call", resourceId);
        msg.methodName = methodName;
        msg.arguments = args;
        return this.getResponse(msg);
    }

    get<T extends UIObject>(type: { new (): T }, resourceId: string): Promise<T> {
        var msg = new RpcMessage("Get", resourceId);
        return this.getResponse(msg).then(source => {
            return this.createFrom(type, source);
        });
    }

    refreshObject(obj: UIObject): Promise<any> {
        var msg = new RpcMessage("Get", obj.resourceId);
        return this.getResponse(msg).then(newState => {
            this.refreshFrom(obj, newState);
        });
    }

    setProperty(obj: UIObject, propName: string, newValue: any) {
        var msg = new RpcMessage("SetProperty", obj.resourceId);
        msg.propertyName = propName;
        msg.value = newValue;
        return this.getResponse(msg).then(null, x => x);
    }

    silentChange: boolean = false;

    refreshFrom(obj: UIObject, source: any) {
        this.silentChange = true;
        for (var srcKey of Object.getOwnPropertyNames(source)) {
            var dstKey = srcKey[0].toLowerCase() + srcKey.substr(1);

            var dstVal = obj[dstKey];
            var dstValType = dstVal && dstVal.type;
            if (Array.isArray(dstVal) && dstValType) {
                var srcArr = source[srcKey];

                var newArr = [];
                for (var i = 0; i < srcArr.length; i++)
                    newArr.push(this.createFrom(dstValType, srcArr[i]));

                dstVal.splice(0, dstVal.length, ...newArr);
            }
            else
                obj[dstKey] = source[srcKey];

            var obs = ko.getObservable(obj, dstKey);
            if (obs == null)
                console.log('obs is null', obj, dstKey);

            obs && obs.subscribe(newValue => {
                if (this.silentChange) return;

                console.log('propChange', obj, dstKey, newValue);
                this.setProperty(obj, dstKey, newValue)
                    .then(x => console.log('setProperty result', x));
            });

            //console.log(`typeof result[${dstKey}]`, typeof result[dstKey], Array.isArray(result[dstKey]));
        }
        this.silentChange = false;
    }

    createFrom<T extends UIObject>(type: { new (): T }, source: any) {
        var result = new type();
        ko.track(result);
        result.jsonRpc = this;

        this.refreshFrom(result, source);
        return result;
    }
}