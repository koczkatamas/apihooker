define(["require", "exports"], function (require, exports) {
    "use strict";
    class ObjectHelper {
        static createFrom(type, source) {
            var result = new type();
            this.copy(source, result);
            return result;
        }
        static copy(src, dst) {
            for (var srcKey of Object.getOwnPropertyNames(src)) {
                var dstKey = srcKey[0].toLowerCase() + srcKey.substr(1);
                dst[dstKey] = src[srcKey];
            }
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = ObjectHelper;
});
//# sourceMappingURL=ObjectHelper.js.map