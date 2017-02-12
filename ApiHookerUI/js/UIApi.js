define(["require", "exports", "./UIObject", "./UIHookableMethod"], function (require, exports, UIObject_1, UIHookableMethod_1) {
    "use strict";
    class UIApi extends UIObject_1.default {
        constructor(jsonRpc, resourceId) {
            super(jsonRpc, resourceId);
            this.jsonRpc = jsonRpc;
            this.resourceId = resourceId;
        }
        echo(message) {
            return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => x);
        }
        getHookableMethods() {
            return this.jsonRpc.call(this.resourceId, "GetHookableMethods", []).
                then(items => items.map(x => UIHookableMethod_1.default.createFrom(x)));
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = UIApi;
    ;
});
//# sourceMappingURL=UIApi.js.map