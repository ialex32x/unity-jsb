
// import { MyEditorWindow } from "./my_editor_window"

import { Rect } from "UnityEngine";

console.log("hello, editor");

setTimeout(function () {
    console.log("hello, editor after 3 secs");
}, 3000);

let fsw = new FSWatcher("Scripts", "*.js");

fsw.oncreate = function (name: string) {
    console.log("new file:", name);
}

fsw.ondelete = function (name: string) {
    console.log("delete file:", name);
}

fsw.onchange = function (name: string) {
    console.log("change file:", name);
}

globalThis["__fsw"] = fsw;

// fffff