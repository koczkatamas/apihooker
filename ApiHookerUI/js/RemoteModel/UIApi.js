define(["require", "exports", "../JsonRpc", "./UIHookableMethod"], function (require, exports, JsonRpc_1, UIHookableMethod_1) {
    "use strict";
    class UIApi extends JsonRpc_1.UIObject {
        constructor() {
            super(...arguments);
            this.hookableMethods = JsonRpc_1.UIObject.typedArray(UIHookableMethod_1.default);
            //getHookableMethods(): Promise<UIHookableMethod[]> {
            //    return this.jsonRpc.call(this.resourceId, "GetHookableMethods", []).
            //        then(items => items.map(x => UIHookableMethod.createFrom(x)));
            //}
        }
        echo(message) {
            return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => x);
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = UIApi;
    ;
});
//# sourceMappingURL=UIApi.js.map