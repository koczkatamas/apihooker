import { JsonRpc, UIObject } from '../JsonRpc';
import UIHookableMethod from './UIHookableMethod';
import UIProcess from "./UIHookedProcess";

export default class UIApi extends UIObject {
    public hookableMethods = UIObject.typedArray(UIHookableMethod);
    public hookedProcesses = UIObject.typedArray(UIProcess);

    echo(message: string): Promise<string> {
        return this.jsonRpc.call(this.resourceId, "Echo", [message]).then(x => <string>x);
    }

    launchAndInject(path: string): Promise<UIProcess> {
        return this.jsonRpc.call(this.resourceId, "LaunchAndInject", [path]).then(x => <UIProcess>x);
    }
};