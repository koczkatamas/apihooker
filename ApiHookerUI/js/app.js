window.onload = function () {
    function startWebSocket() {
        socket = new WebSocket("ws://127.0.0.1:1338/");
        socket.onmessage = function (event) { return console.log('websocket msg', event.data); };
        socket.onopen = function (openEvent) {
            console.log('websocket connected.');
            onWsConnected();
        };
        socket.onerror = function (errorEvent) {
            console.log('websocket error', errorEvent, socket);
            if (socket.readyState === WebSocket.CLOSED)
                return;
        };
        socket.onclose = function (closeEvent) {
            console.log('websocket auto reconnecting...', closeEvent);
            setTimeout(startWebSocket, 500);
        };
    }
    startWebSocket();
    function onWsConnected() {
        socket.send(JSON.stringify({ messageType: "call", resourceId: "api", methodName: "GetHookableMethods" }));
    }
};
//# sourceMappingURL=app.js.map