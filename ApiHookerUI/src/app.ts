declare var socket: WebSocket;
window.onload = () => {
    function startWebSocket() {
        socket = new WebSocket("ws://127.0.0.1:1338/");
        socket.onmessage = event => console.log('weboscket msg', event.data);
        socket.onopen = openEvent => {
            console.log('websocket connected.');
            onWsConnected();
        };
        socket.onerror = errorEvent => {
            console.log('websocket error', errorEvent, socket);
            if (socket.readyState === WebSocket.CLOSED) return;
        };
        socket.onclose = closeEvent => {
            console.log('websocket auto reconnecting...', closeEvent);
            setTimeout(startWebSocket, 500);
        };
    }

    startWebSocket();

    function onWsConnected() {
        socket.send(JSON.stringify({ messageType: "call", resourceId: "api", methodName: "GetHookableMethods" }));
    }
};