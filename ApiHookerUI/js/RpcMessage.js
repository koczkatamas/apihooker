define(["require", "exports", "./ObjectHelper"], function (require, exports, ObjectHelper_1) {
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
            return ObjectHelper_1.default.createFrom(RpcMessage, JSON.parse(source));
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = RpcMessage;
});
//# sourceMappingURL=RpcMessage.js.map