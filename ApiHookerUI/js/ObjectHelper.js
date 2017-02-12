define(["require", "exports"], function (require, exports) {
    "use strict";
    var ObjectHelper;
    (function (ObjectHelper) {
        function createFrom(type, source) {
            var result = new type();
            this.copy(source, result);
            return result;
        }
        ObjectHelper.createFrom = createFrom;
        function copy(src, dst) {
            for (var srcKey of Object.getOwnPropertyNames(src)) {
                var dstKey = srcKey[0].toLowerCase() + srcKey.substr(1);
                dst[dstKey] = src[srcKey];
            }
        }
        ObjectHelper.copy = copy;
    })(ObjectHelper = exports.ObjectHelper || (exports.ObjectHelper = {}));
});
//# sourceMappingURL=ObjectHelper.js.map