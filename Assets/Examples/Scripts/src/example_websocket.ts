
export function run() {
    let ws = new WebSocket("ws://192.168.0.1:8080/websocket", "default");

    console.log("ws.readyState:", ws.readyState);

    ws.onopen = function () {
        console.log("ws.onopen", ws.readyState);
        setTimeout(function () {
            ws.send("test")
        }, 1000);
    };
    ws.onclose = function () {
        console.log("ws.onclose", ws.readyState);
    };
    ws.onerror = function () {
        console.log("ws.onerror", ws.readyState);
    };
    ws.onmessage = function (msg) {
        console.log("ws.onmessage", msg);
    };
}
