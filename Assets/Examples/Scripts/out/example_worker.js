let worker = new Worker("worker");
worker.onmessage = function (data) {
    console.log("master receive message from worker:", data);
};
setInterval(function () {
    worker.postMessage("hello, worker! i am master!");
}, 45000);
//# sourceMappingURL=example_worker.js.map