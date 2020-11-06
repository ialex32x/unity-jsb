"use strict";
// import { MyEditorWindow } from "./my_editor_window"
Object.defineProperty(exports, "__esModule", { value: true });
console.log("hello, editor");
setTimeout(function () {
    console.log("hello, editor after 3 secs");
}, 3000);
let fsw = new FSWatcher("Scripts", "*.js");
fsw.oncreate = function (name) {
    console.log("new file:", name);
};
fsw.ondelete = function (name) {
    console.log("delete file:", name);
};
fsw.onchange = function (name) {
    console.log("change file:", name);
};
globalThis["__fsw"] = fsw;
// fffff
//# sourceMappingURL=main.js.map