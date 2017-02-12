export default class ObjectHelper {
    static createFrom<T>(type: { new (): T; }, source: any): T {
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