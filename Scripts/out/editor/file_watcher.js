"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.FileWatcher = exports.EFileState = void 0;
const dispatcher_1 = require("../events/dispatcher");
var EFileState;
(function (EFileState) {
    EFileState[EFileState["CHANGE"] = 1] = "CHANGE";
    EFileState[EFileState["NEW"] = 2] = "NEW";
    EFileState[EFileState["DELETE"] = 3] = "DELETE";
})(EFileState = exports.EFileState || (exports.EFileState = {}));
class FileWatcher {
    constructor(path, filter) {
        this._dispatcher = new dispatcher_1.EventDispatcher();
        this._disposed = false;
        this._pending = false;
        this._cache = {};
        this._fsw = new FSWatcher(path, filter);
        this._fsw.oncreate = this.oncreate.bind(this);
        this._fsw.onchange = this.onchange.bind(this);
        this._fsw.ondelete = this.ondelete.bind(this);
        this._fsw.includeSubdirectories = true;
        this._fsw.enableRaisingEvents = true;
    }
    dispose() {
        if (this._disposed) {
            return;
        }
        this._disposed = true;
        this._fsw.dispose();
        this._fsw = null;
    }
    on(name, caller, fn) {
        this._dispatcher.on(name, caller, fn);
    }
    off(name, caller, fn) {
        this._dispatcher.off(name, caller, fn);
    }
    oncreate(name, fullPath) {
        this.setCacheState(name, fullPath, EFileState.NEW);
    }
    onchange(name, fullPath) {
        this.setCacheState(name, fullPath, EFileState.CHANGE);
    }
    ondelete(name, fullPath) {
        this.setCacheState(name, fullPath, EFileState.DELETE);
    }
    setCacheState(name, fullPath, state) {
        if (this._disposed) {
            return;
        }
        this._cache[name] = {
            name: name,
            fullPath: fullPath,
            state: state,
        };
        if (!this._pending) {
            this._pending = true;
            setTimeout(() => this.dispatchEvents(), 500);
        }
    }
    dispatchEvents() {
        if (this._disposed) {
            return;
        }
        this._pending = false;
        let map = this._cache;
        this._cache = {};
        for (let name in map) {
            let state = map[name];
            this._dispatcher.dispatch(name, state);
            this._dispatcher.dispatch("*", state);
        }
    }
}
exports.FileWatcher = FileWatcher;
//# sourceMappingURL=file_watcher.js.map