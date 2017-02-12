import ObjectHelper from "./ObjectHelper";

export default class UIHookableMethod {
    public static createFrom(source: any) {
        return ObjectHelper.createFrom(UIHookableMethod, source);
    }
}