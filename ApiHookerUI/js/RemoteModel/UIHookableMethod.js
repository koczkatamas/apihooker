define(["require", "exports", "../ObjectHelper", "../JsonRpc"], function (require, exports, ObjectHelper_1, JsonRpc_1) {
    "use strict";
    class UIHookableMethod extends JsonRpc_1.UIObject {
        constructor() {
            super(...arguments);
            this.name = '';
            this.hookIt = false;
        }
        static createFrom(source) {
            return ObjectHelper_1.ObjectHelper.createFrom(UIHookableMethod, source);
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = UIHookableMethod;
});
//# sourceMappingURL=UIHookableMethod.js.map