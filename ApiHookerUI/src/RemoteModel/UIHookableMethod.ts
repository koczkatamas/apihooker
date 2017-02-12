import { ObjectHelper } from "../ObjectHelper";
import {UIObject} from "../JsonRpc";

export default class UIHookableMethod extends UIObject {
    public name: string = '';
    public hookIt: boolean = false;

    public static createFrom(source: any) {
        return ObjectHelper.createFrom(UIHookableMethod, source);
    }
}