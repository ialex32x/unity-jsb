console.log("in worker");
setInterval(function () {
    // console.log("worker log");
    postMessage("message form worker");
}, 3000);
onmessage = function (data) {
    console.log("worker get message from master:", data);
};
//# sourceMappingURL=worker.js.map