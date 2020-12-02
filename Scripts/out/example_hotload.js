"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const System_IO_1 = require("System.IO");
const file_watcher_1 = require("./editor/file_watcher");
let data = JSON.parse(System_IO_1.File.ReadAllText("Scripts/config/data.json"));
let fw = new file_watcher_1.FileWatcher("Scripts/config", "*.json");
function show_data() {
    console.log("read data: ", data.id, data.name);
}
fw.includeSubdirectories = false;
fw.on("*", this, function (filestate) {
    let fi = new System_IO_1.FileInfo(filestate.fullPath);
    if (fi.Name == "data.json") {
        data = JSON.parse(System_IO_1.File.ReadAllText("Scripts/config/data.json"));
        show_data();
    }
});
// 阻止 fw 被 gc
globalThis["fw"] = fw;
show_data();
//# sourceMappingURL=example_hotload.js.map