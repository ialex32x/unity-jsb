let worker = new Worker("worker");
let messageId = 0;
let messageQueue = {};
//TODO: 目前无法传递 C# 对象 (对象引用ID不共享)
//TODO: 传递 object 崩溃
worker.onmessage = function (data_t) {
    let data = JSON.parse(data_t);
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
        worker.postMessage(JSON.stringify({
            "id": id,
            "method": "add",
            "args": [a, b]
        }));
    });
}
console.log("test");
test();
setInterval(() => test(), 3000);
//# sourceMappingURL=example_worker.js.map