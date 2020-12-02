import { File, FileInfo } from "System.IO";
import { FileState, FileWatcher } from "./editor/file_watcher";

let data = JSON.parse(File.ReadAllText("Scripts/config/data.json"));
let fw = new FileWatcher("Scripts/config", "*.json");

function show_data() {
    console.log("read data: ", data.id, data.name);
}

fw.includeSubdirectories = false;
fw.on("*", this, function (filestate: FileState) {
    let fi = new FileInfo(filestate.fullPath);
    if (fi.Name == "data.json") {
        data = JSON.parse(File.ReadAllText("Scripts/config/data.json"));
        show_data();
    }
});

// 阻止 fw 被 gc
globalThis["fw"] = fw;
show_data();
