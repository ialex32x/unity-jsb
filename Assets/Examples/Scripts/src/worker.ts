

console.log("in worker");

setInterval(function () {
    console.log("worker log");
}, 3000)

onmessage = function (data) {
    console.log("worker get message:", data);
}
