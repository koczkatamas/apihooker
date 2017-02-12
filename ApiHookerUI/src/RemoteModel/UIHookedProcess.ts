import { ObjectHelper } from "../ObjectHelper";
import { UIObject } from "../JsonRpc";

export default class UIProcess extends UIObject {
    public name: string = '';

    readNewCallRecords(): Promise<any> {
        return this.jsonRpc.call(this.resourceId, "ReadNewCallRecords", []);
    }

    unhookAndWaitForExit(): Promise<any> {
        return this.jsonRpc.call(this.resourceId, "UnhookAndWaitForExit", []);
    }
}