
export class Handler {
    caller: any;
    fn: Function;
    once: boolean;

    constructor(caller: any, fn: Function, once?: boolean) {
        this.caller = caller;
        this.fn = fn;
        this.once = !!once;
    }

    invoke(arg0?: any, arg1?: any, arg2?: any) {
        if (this.fn) {
            this.fn.call(this.caller, arg0, arg1, arg2);
        }
    }
}

/**
 * 简单的事件分发器实现
 * 此实现功能与 DuktapeJS.Dispatcher 基本一致, 
 * 但 DuktapeJS.Dispatcher 不保证事件响应顺序, 但效率更高 (因为复用了中途移除的索引)
 */
export class Dispatcher {
    private _handlers: Array<Handler> = [];

    on(caller: any, fn: Function) {
        let handler = new Handler(caller, fn);

        this._handlers.push(handler);
        return handler;
    }

    once(caller: any, fn: Function) {
        let handler = new Handler(caller, fn, true);

        this._handlers.push(handler);
        return handler;
    }

    off(caller: any, fn: Function) {
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
                } else {
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

    dispatch(arg0?: any, arg1?: any, arg2?: any) {
        let size = this._handlers.length;
        if (size == 0) {
            return;
        }
        if (size == 1) {
            let item = this._handlers[0];
            if (item.once) {
                this._handlers.splice(0, 1);
            }
            item.invoke(arg0, arg1, arg2);
            return;
        }
        if (size == 2) {
            let item0 = this._handlers[0];
            let item1 = this._handlers[1];
            if (item0.once) {
                if (item1.once) {
                    this._handlers.splice(0, 2);
                } else {
                    this._handlers.splice(0, 1);
                }
            } else {
                if (item1.once) {
                    this._handlers.splice(1, 1);
                } 
            }
            item0.invoke(arg0, arg1, arg2);
            item1.invoke(arg0, arg1, arg2);
            return;
        }
        let copy = new Array<Handler>(...this._handlers);
        for (let i = 0; i < size; i++) {
            let item = copy[i];
            if (item.once) {
                let found = this._handlers.indexOf(item);
                if (found >= 0) {
                    this._handlers.splice(found, 1);
                }
            }
            copy[i].invoke(arg0, arg1, arg2);
        }
    }
}

/**
 * 按事件名派发
 */
export class EventDispatcher {
    private _dispatcher: { [key: string]: Dispatcher } = {};

    on(evt: string, caller: any, fn?: Function) {
        let dispatcher = this._dispatcher[evt];

        if (typeof dispatcher === "undefined") {
            dispatcher = this._dispatcher[evt] = new Dispatcher();
        }
        dispatcher.on(caller, fn);
    }

    once(evt: string, caller: any, fn?: Function) {
        let dispatcher = this._dispatcher[evt];

        if (typeof dispatcher === "undefined") {
            dispatcher = this._dispatcher[evt] = new Dispatcher();
        }
        dispatcher.once(caller, fn);
    }

    off(evt: string, caller: any, fn?: Function) {
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
    dispatch(evt: string, arg0?: any, arg1?: any, arg2?: any) {
        let dispatcher = this._dispatcher[evt];

        if (typeof dispatcher !== "undefined") {
            dispatcher.dispatch(arg0, arg1, arg2);
        }
    }
}
