define(["require", "exports", "./RpcMessage"], function (require, exports, RpcMessage_1) {
    "use strict";
    class JsonRpc {
        constructor(socket) {
            this.socket = socket;
            this.nextMessageId = 0;
            this.msgHandlers = {};
            socket.onmessage = event => {
                var responseMsg = RpcMessage_1.default.parse(event.data);
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
                var msg = new RpcMessage_1.default();
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
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = JsonRpc;
});
//# sourceMappingURL=JsonRpc.js.map