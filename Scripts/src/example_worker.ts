
let worker = new Worker("worker");
let messageId = 0;
let messageQueue: { [k: number]: Function } = {};

worker.onmessage = function (data: any) {
    let p = messageQueue[data.id];
    if (typeof p !== "undefined") {
        p(data.result);
    }
}

let a = 1;
let b = 2;

async function test() {
    a++;
    b++;
    let r = await remoteAdd(a, b);
    console.log(`add(${a}, ${b}) = ${r}`);
}

async function remoteAdd(a: number, b: number): Promise<number> {
    let id = messageId++;
    return new Promise(resolve => {
        messageQueue[id] = resolve;
        worker.postMessage({
            "id": id,
            "method": "add",
            "args": [a, b]
        });
    });
}


console.log("test");
setInterval(() => test(), 3000);
