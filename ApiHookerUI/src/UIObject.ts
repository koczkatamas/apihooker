import JsonRpc from "./JsonRpc";

export default class UIObject {
    constructor(public jsonRpc: JsonRpc, public resourceId: string){ }
}