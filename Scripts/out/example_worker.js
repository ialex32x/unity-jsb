let worker = new Worker("worker");
let messageId = 0;
let messageQueue = {};
worker.onmessage = function (data) {
    let p = messageQueue[data.id];
    if (typeof p !== "undefined") {
        p(data.result);
    }
};
let a = 1;
let b = 2;
async function test() {
    a++;
    b++;
    let r = await remoteAdd(a, b);
    console.log(`add(${a}, ${b}) = ${r}`);
}
async function remoteAdd(a, b) {
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
//# sourceMappingURL=example_worker.js.map