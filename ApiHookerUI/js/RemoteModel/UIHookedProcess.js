define(["require", "exports", "../JsonRpc"], function (require, exports, JsonRpc_1) {
    "use strict";
    class UIProcess extends JsonRpc_1.UIObject {
        constructor() {
            super(...arguments);
            this.name = '';
        }
        readNewCallRecords() {
            return this.jsonRpc.call(this.resourceId, "ReadNewCallRecords", []);
        }
        unhookAndWaitForExit() {
            return this.jsonRpc.call(this.resourceId, "UnhookAndWaitForExit", []);
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = UIProcess;
});
//# sourceMappingURL=UIHookedProcess.js.map