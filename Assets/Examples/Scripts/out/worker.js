console.log("in worker");
let i = 0;
setInterval(function () {
    // console.log("worker log");
    postMessage("message form worker" + (i++));
}, 3000);
onmessage = function (data) {
    console.log("worker get message from master:", data);
};
//# sourceMappingURL=worker.js.map