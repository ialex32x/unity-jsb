
export abstract class Subscriber {
    private _model: DataBinding;
    private _key: string;
    private _source: boolean;

    constructor(model: DataBinding, key: string) {
        this._model = model;
        this._key = key;
        this._model.addSubscriber(this);
    }

    get value() {
        return this._model[this._key];
    }

    set value(newValue) {
        this._source = true;
        this._model[this._key] = newValue;
        this._source = false;
    }

    protected update(value: any) {
    }

    notify(value: any) {
        if (!this._source) {
            this.update(value);
        }
    }

    unsubscribe() {
        if (this._model) {
            this._model.removeSubscriber(this);
            this._model = undefined;
        }
    }
}

export class Subscribers {
    private _subs: Array<Subscriber>;

    notify(valueProxy: any) {
        if (this._subs) {
            const copy = this._subs.slice();
            for (let i = 0, len = copy.length; i < len; i++) {
                copy[i].notify(valueProxy);
            }
        }
    }

    addSub(sub: Subscriber) {
        if (!this._subs) {
            this._subs = [];
        }
        this._subs.push(sub);
    }

    removeSub(sub: Subscriber) {
        if (this._subs && this._subs.length) {
            const index = this._subs.indexOf(sub);
            if (index >= 0) {
                this._subs.splice(index, 1);
            }
        }
    }

    // 废弃当前值, 将监听者转移给新值
    transfer(newValue: Subscribers) {
        newValue._subs = this._subs;
        this._subs = undefined;
    }
}

const SubscribersKey = Symbol.for("subscribers");

export class DataBinding {
    private constructor() {
        Object.defineProperty(this, SubscribersKey, { value: new Subscribers(), enumerable: false });
    }

    addSubscriber(sub: Subscriber) {
        this[SubscribersKey].addSub(sub);
    }

    removeSubscriber(sub: Subscriber) {
        this[SubscribersKey].removeSub(sub);
    }

    static bind<T>(data: T): T {
        let model = new DataBinding();
        let subscribers = model[SubscribersKey];
        for (let key in data) {
            if (key.startsWith("$") || key.startsWith("_$")) {
                continue;
            }
            let value = data[key];
            let valueProxy = value;

            if (typeof value === "object") {
                valueProxy = DataBinding.bind(value);
            }

            Object.defineProperty(model, key, {
                enumerable: true,
                get() {
                    return valueProxy;
                },
                set(newValue) {
                    if (newValue !== value) {
                        let oldValue = value;
                        if (typeof newValue === "object") {
                            valueProxy = DataBinding.bind(newValue);
                            oldValue[SubscribersKey].transfer(valueProxy[SubscribersKey]);
                            // Model.transfer(<Model><any>oldValue, <Model><any>valueProxy);
                        } else {
                            valueProxy = newValue;
                        }
                        subscribers.notify(valueProxy);
                    }
                },
            });
        }
        return <T><any>model;
    }

    static subscribe<T extends Subscriber>(SubscriberType: { new(model: DataBinding, key: string, ...args): T }, modelObject: any, path: string, ...args): T {
        let model = <DataBinding>modelObject;
        let keys = path.split(".");
        let key = path;
        for (let i = 0, len = keys.length - 1; i < len; i++) {
            key = keys[i];
            model = model[key];
        }
        let sub = new SubscriberType(model, key, ...args);
        return <T>sub;
    }
}
