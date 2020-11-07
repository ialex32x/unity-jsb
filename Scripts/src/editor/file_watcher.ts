import { EventDispatcher } from "../events/dispatcher";

export enum EFileState {
    CHANGE = 1,
    NEW = 2,
    DELETE = 3,
}

export interface FileState {
    name: string
    state: EFileState
}

export class FileWatcher {
    private _fsw: FSWatcher;
    private _dispatcher: EventDispatcher = new EventDispatcher();
    private _disposed = false;

    private _pending = false;
    private _cache: { [name: string]: FileState };

    constructor(path: string, filter: string) {
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

    on(name: string, caller: any, fn: Function) {
        this._dispatcher.on(name, caller, fn);
    }

    off(name: string, caller: any, fn: Function) {
        this._dispatcher.off(name, caller, fn);
    }

    private oncreate(name: string) {
        this.setCacheState(name, EFileState.NEW);
    }

    private onchange(name: string) {
        this.setCacheState(name, EFileState.CHANGE);
    }

    private ondelete(name: string) {
        this.setCacheState(name, EFileState.DELETE);
    }

    private setCacheState(name: string, state: EFileState) {
        if (this._disposed) {
            return;
        }

        this._cache[name] = {
            name: name,
            state: state,
        };
        if (!this._pending) {
            this._pending = true;
            setTimeout(() => this.dispatchEvents(), 2000);
        }
    }

    private dispatchEvents() {
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