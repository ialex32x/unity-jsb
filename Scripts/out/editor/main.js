"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const file_watcher_1 = require("./file_watcher");
console.log("hello, editor");
setTimeout(function () {
    console.log("hello, editor after 3 secs");
}, 3000);
// let fsw = new FSWatcher("Scripts", "*.js");
// fsw.oncreate = function (name: string) {
//     console.log("new file:", name);
// }
// fsw.ondelete = function (name: string) {
//     console.log("delete file:", name);
// }
// fsw.onchange = function (name: string) {
//     console.log("change file:", name);
// }
// globalThis["__fsw"] = fsw;
if (typeof globalThis["__fw"] !== "undefined") {
    globalThis["__fw"].dispose();
    delete globalThis["__fw"];
}
let fw = new file_watcher_1.FileWatcher("Scripts", "*.js");
fw.on("*", this, function (filestate) {
    if (typeof require.cache[filestate.name] !== "undefined") {
        delete require.cache[filestate.name];
        console.log("reload module", filestate.name);
    }
});
globalThis["__fw"] = fw;
Object.keys(require.cache).forEach(k => console.log("require.cache entry:", k));
//# sourceMappingURL=main.js.map