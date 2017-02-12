define(["require", "exports"], function (require, exports) {
    "use strict";
    class WebSocketHandler {
        constructor(url) {
            this.url = url;
            this.onConnected = null;
        }
        start() {
            this.socket = new WebSocket(this.url);
            this.socket.onopen = openEvent => {
                console.log('websocket connected.');
                this.onConnected && this.onConnected(this.socket);
            };
            this.socket.onerror = errorEvent => {
                console.log('websocket error', errorEvent, this.socket);
                if (this.socket.readyState === WebSocket.CLOSED)
                    return;
            };
            this.socket.onclose = closeEvent => {
                console.log('websocket auto reconnecting...', closeEvent);
                setTimeout(() => this.start(), 500);
            };
        }
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = WebSocketHandler;
});
//# sourceMappingURL=WebSocketHandler.js.map