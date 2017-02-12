import JsonRpc from './JsonRpc';
import UIObject from './UIObject';
import UIHookableMethod from './UIHookableMethod';

export default class UIApi extends UIObject {
    constructor(public jsonRpc: JsonRpc, public resourceId: string) {
        super(jsonRpc, resourceId);
    }

    echo(message: string): Promise<string> {
        return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => <string>x);
    }

    getHookableMethods(): Promise<UIHookableMethod[]> {
        return this.jsonRpc.call(this.resourceId, "GetHookableMethods", []).
            then(items => items.map(x => UIHookableMethod.createFrom(x)));
    }
};