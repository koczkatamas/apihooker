import * as Api from "./RemoteModel/UIApi";

export default class WebSocketHandler {
    public socket: WebSocket;
    public onConnected: ((socket: WebSocket) => void) = null;

    constructor(public url: string) { }

    public start() {
        this.socket = new WebSocket(this.url);

        this.socket.onopen = openEvent => {
            console.log('websocket connected.');
            this.onConnected && this.onConnected(this.socket);
        };

        this.socket.onerror = errorEvent => {
            console.log('websocket error', errorEvent, this.socket);
            if (this.socket.readyState === WebSocket.CLOSED) return;
        };

        this.socket.onclose = closeEvent => {
            console.log('websocket auto reconnecting...', closeEvent);
            setTimeout(() => this.start(), 500);
        };
    }
}