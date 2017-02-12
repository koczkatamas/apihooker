import RpcMessage from './RpcMessage';

export default class JsonRpc {
    nextMessageId = 0;
    msgHandlers: { [msgId: string]: { resolve; reject } } = { };

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
}