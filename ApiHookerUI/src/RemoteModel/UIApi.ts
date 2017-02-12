import { JsonRpc, UIObject } from '../JsonRpc';
import UIHookableMethod from './UIHookableMethod';

export default class UIApi extends UIObject {
    public hookableMethods = UIObject.typedArray(UIHookableMethod);

    echo(message: string): Promise<string> {
        return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => <string>x);
    }

    //getHookableMethods(): Promise<UIHookableMethod[]> {
    //    return this.jsonRpc.call(this.resourceId, "GetHookableMethods", []).
    //        then(items => items.map(x => UIHookableMethod.createFrom(x)));
    //}
};