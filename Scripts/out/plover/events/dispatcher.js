"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.EventDispatcher = exports.Dispatcher = exports.Handler = void 0;
class Handler {
    constructor(caller, fn) {
        this.caller = caller;
        this.fn = fn;
    }
    invoke(arg0, arg1, arg2) {
        if (this.fn) {
            this.fn.call(this.caller, arg0, arg1, arg2);
        }
    }
}
exports.Handler = Handler;
/**
 * 简单的事件分发器实现
 * 此实现功能与 DuktapeJS.Dispatcher 基本一致,
 * 但 DuktapeJS.Dispatcher 不保证事件响应顺序, 但效率更高 (因为复用了中途移除的索引)
 */
class Dispatcher {
    constructor() {
        this._handlers = [];
    }
    on(caller, fn) {
        let handler = new Handler(caller, fn);
        this._handlers.push(handler);
        return handler;
    }
    off(caller, fn) {
        let size = this._handlers.length;
        if (typeof fn === "undefined") {
            let found = false;
            for (let i = 0; i < size;) {
                let item = this._handlers[i];
                if (item.caller == caller) {
                    found = true;
                    item.fn = null;
                    item.caller = null;
                    this._handlers.splice(i, 1);
                    size--;
                }
                else {
                    i++;
                }
            }
            return found;
        }
        for (let i = 0; i < size; i++) {
            let item = this._handlers[i];
            if (item.caller == caller && item.fn == fn) {
                item.fn = null;
                item.caller = null;
                this._handlers.splice(i, 1);
                return true;
            }
        }
        return false;
    }
    /**
     * 移除所有处理器
     */
    clear() {
        this._handlers.splice(0);
    }
    dispatch(arg0, arg1, arg2) {
        let size = this._handlers.length;
        if (size == 0) {
            return;
        }
        if (size == 1) {
            let item = this._handlers[0];
            item.invoke(arg0, arg1, arg2);
            return;
        }
        if (size == 2) {
            let item0 = this._handlers[0];
            let item1 = this._handlers[1];
            item0.invoke(arg0, arg1, arg2);
            item1.invoke(arg0, arg1, arg2);
            return;
        }
        let copy = new Array(...this._handlers);
        for (let i = 0; i < size; i++) {
            copy[i].invoke(arg0, arg1, arg2);
        }
    }
}
exports.Dispatcher = Dispatcher;
/**
 * 按事件名派发
 */
class EventDispatcher {
    constructor() {
        this._dispatcher = {};
    }
    on(evt, caller, fn) {
        let dispatcher = this._dispatcher[evt];
        if (typeof dispatcher === "undefined") {
            dispatcher = this._dispatcher[evt] = new Dispatcher();
        }
        dispatcher.on(caller, fn);
    }
    off(evt, caller, fn) {
        let dispatcher = this._dispatcher[evt];
        if (typeof dispatcher !== "undefined") {
            dispatcher.off(caller, fn);
        }
    }
    clear() {
        for (let evt in this._dispatcher) {
            let dispatcher = this._dispatcher[evt];
            if (dispatcher instanceof Dispatcher) {
                dispatcher.clear();
            }
        }
    }
    /**
     * 派发指定事件
     */
    dispatch(evt, arg0, arg1, arg2) {
        let dispatcher = this._dispatcher[evt];
        if (typeof dispatcher !== "undefined") {
            dispatcher.dispatch(arg0, arg1, arg2);
        }
    }
}
exports.EventDispatcher = EventDispatcher;
//# sourceMappingURL=dispatcher.js.map