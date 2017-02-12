define(["require", "exports", "../JsonRpc", "./UIHookableMethod", "./UIHookedProcess"], function (require, exports, JsonRpc_1, UIHookableMethod_1, UIHookedProcess_1) {
    "use strict";
    class UIApi extends JsonRpc_1.UIObject {
        constructor() {
            super(...arguments);
            this.hookableMethods = JsonRpc_1.UIObject.typedArray(UIHookableMethod_1.default);
            this.hookedProcesses = JsonRpc_1.UIObject.typedArray(UIHookedProcess_1.default);
        }
        echo(message) {
            return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => x);
        }
        launchAndHook(path) {
            return this.jsonRpc.call(this.resourceId, "LaunchAndHook", [path])
                .then(x => this.jsonRpc.createFrom(UIHookedProcess_1.default, x));
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = UIApi;
    ;
});
//# sourceMappingURL=UIApi.js.map