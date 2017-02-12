import ObjectHelper from "./ObjectHelper";

export default class RpcMessage {
    public messageType: "Error" | "Call" | "CallResponse";
    public error: "UnexpectedError" | "NoError" | "UnknownMessageType" | "ResourceNotFound" | "MethodNotFound" | "ArgumentCountMismatch" | "UnknownArgumentType" | "NotAllowedOrigin";
    public messageId: string;
    public resourceId: string;
    public methodName: string;
    public arguments: any[];
    public value: any;

    public serialize(): string {
        return JSON.stringify(this);
    }

    public static parse(source: any) {
        return ObjectHelper.createFrom(RpcMessage, JSON.parse(source));
    }
}