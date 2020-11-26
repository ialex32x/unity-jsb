"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
onmessage = function (data) {
    switch (data.method) {
        case "add": {
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