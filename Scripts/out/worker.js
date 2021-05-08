onmessage = function (e) {
    let data = e.data;
    switch (data.method) {
        case "add": {
            let x = 0;
            let s = Date.now();
            // 这里假装要处理很久很久
            for (let i = 0; i < 10000000; i++) {
                x++;
            }
            let d = Date.now() - s;
            console.log("[worker] duration:", d, "repeated:", x);
            postMessage({
                "id": data.id,
                "result": data.args[0] + data.args[1],
            });
            break;
        }
        default: {
            postMessage({
                "id": data.id,
                "result": "unknown method",
            });
            break;
        }
    }
};
//# sourceMappingURL=worker.js.map