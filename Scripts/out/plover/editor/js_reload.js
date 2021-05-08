"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.reload = void 0;
const file_watcher_1 = require("./file_watcher");
const jsb_1 = require("jsb");
let FileWatcherSymbol = Symbol.for("GlobalFileWatcher");
if (typeof globalThis[FileWatcherSymbol] !== "undefined") {
    globalThis[FileWatcherSymbol].dispose();
    delete globalThis[FileWatcherSymbol];
}
let fw = new file_watcher_1.FileWatcher("Scripts", "*.js");
function reload(mod) {
    if (typeof mod === "object") {
        let dirtylist = [];
        collect_reload_deps(mod, dirtylist);
        do_reload(dirtylist);
    }
}
exports.reload = reload;
function do_reload(dirtylist) {
    if (dirtylist.length > 0) {
        jsb_1.ModuleManager.BeginReload();
        for (let i = 0; i < dirtylist.length; i++) {
            let mod = dirtylist[i];
            console.warn("reloading", mod.id);
            jsb_1.ModuleManager.MarkReload(mod.id);
        }
        jsb_1.ModuleManager.EndReload();
    }
}
function collect_reload_deps(mod, dirtylist) {
    if (dirtylist.indexOf(mod) < 0) {
        dirtylist.push(mod);
        let parent = mod.parent;
        if (typeof parent === "object") {
            collect_reload_deps(parent, dirtylist);
            parent = parent.parent;
        }
    }
}
fw.on(file_watcher_1.FileWatcher.CHANGED, this, function (filestates) {
    let cache = require.main["cache"];
    let dirtylist = [];
    for (let name in filestates) {
        let filestate = filestates[name];
        // console.log("file changed:", filestate.name, filestate.fullPath, filestate.state);
        for (let moduleId in cache) {
            let mod = cache[moduleId];
            // console.warn(mod.filename, mod.filename == filestate.fullPath)
            if (mod.filename == filestate.fullPath) {
                collect_reload_deps(mod, dirtylist);
                break;
            }
        }
    }
    do_reload(dirtylist);
});
globalThis[FileWatcherSymbol] = fw;
console.log("i am here");
//# sourceMappingURL=js_reload.js.map