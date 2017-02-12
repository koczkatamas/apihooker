define(["require", "exports", "./ObjectHelper", "knockout"], function (require, exports, ObjectHelper_1, ko) {
    "use strict";
    class RpcMessage {
        constructor(messageType, resourceId) {
            this.messageType = messageType;
            this.resourceId = resourceId;
        }
        serialize() {
            return JSON.stringify(this);
        }
        static parse(source) {
            return ObjectHelper_1.ObjectHelper.createFrom(RpcMessage, JSON.parse(source));
        }
    }
    exports.RpcMessage = RpcMessage;
    class UIObject {
        constructor(jsonRpc = null, resourceId = '') {
            this.jsonRpc = jsonRpc;
            this.resourceId = resourceId;
        }
        static typedArray(type) {
            var result = [];
            result.type = type;
            return result;
        }
    }
    exports.UIObject = UIObject;
    class JsonRpc {
        constructor(socket) {
            this.socket = socket;
            this.nextMessageId = 0;
            this.msgHandlers = {};
            this.silentChange = false;
            socket && this.setSocket(socket);
        }
        setSocket(socket) {
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
        getResponse(msg) {
            return new Promise((resolve, reject) => {
                msg.messageId = msg.messageId || `msg_${this.nextMessageId++}`;
                this.msgHandlers[msg.messageId] = { resolve, reject };
                console.log('[RPC] send', msg);
                var msgJson = msg.serialize();
                this.socket.send(msgJson);
            });
        }
        call(resourceId, methodName, args) {
            var msg = new RpcMessage("Call", resourceId);
            msg.methodName = methodName;
            msg.arguments = args;
            return this.getResponse(msg);
        }
        get(type, resourceId) {
            var msg = new RpcMessage("Get", resourceId);
            return this.getResponse(msg).then(source => {
                return this.createFrom(type, source);
            });
        }
        refreshObject(obj) {
            var msg = new RpcMessage("Get", obj.resourceId);
            return this.getResponse(msg).then(newState => {
                this.refreshFrom(obj, newState);
            });
        }
        setProperty(obj, propName, newValue) {
            var msg = new RpcMessage("SetProperty", obj.resourceId);
            msg.propertyName = propName;
            msg.value = newValue;
            return this.getResponse(msg).then(null, x => x);
        }
        refreshFrom(obj, source) {
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
                    if (this.silentChange)
                        return;
                    console.log('propChange', obj, dstKey, newValue);
                    this.setProperty(obj, dstKey, newValue)
                        .then(x => console.log('setProperty result', x));
                });
            }
            this.silentChange = false;
        }
        createFrom(type, source) {
            var result = new type();
            ko.track(result);
            result.jsonRpc = this;
            this.refreshFrom(result, source);
            return result;
        }
    }
    exports.JsonRpc = JsonRpc;
});
//# sourceMappingURL=JsonRpc.js.map