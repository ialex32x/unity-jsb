import { Sleep } from "jsb";

onmessage = function (data_t: any) {
    let data = JSON.parse(data_t);
    switch (data.method) {
        case "add": {
            postMessage(JSON.stringify({
                "id": data.id,
                "result": data.args[0] + data.args[1],
            }));
            break;
        }
        default: {
            postMessage(JSON.stringify({
                "id": data.id, 
                "result": "unknown method",
            }));
            break;
        }
    }
}
