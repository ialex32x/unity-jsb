"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class Subscriber {
    constructor(model, key) {
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
    update(value) {
    }
    notify(value) {
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
exports.Subscriber = Subscriber;
class Subscribers {
    notify(valueProxy) {
        if (this._subs) {
            const copy = this._subs.slice();
            for (let i = 0, len = copy.length; i < len; i++) {
                copy[i].notify(valueProxy);
            }
        }
    }
    addSub(sub) {
        if (!this._subs) {
            this._subs = [];
        }
        this._subs.push(sub);
    }
    removeSub(sub) {
        if (this._subs && this._subs.length) {
            const index = this._subs.indexOf(sub);
            if (index >= 0) {
                this._subs.splice(index, 1);
            }
        }
    }
    // 废弃当前值, 将监听者转移给新值
    transfer(newValue) {
        newValue._subs = this._subs;
        this._subs = undefined;
    }
}
exports.Subscribers = Subscribers;
const SubscribersKey = Symbol.for("subscribers");
class DataBinding {
    constructor() {
        Object.defineProperty(this, SubscribersKey, { value: new Subscribers(), enumerable: false });
    }
    addSubscriber(sub) {
        this[SubscribersKey].addSub(sub);
    }
    removeSubscriber(sub) {
        this[SubscribersKey].removeSub(sub);
    }
    static bind(data) {
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
                        }
                        else {
                            valueProxy = newValue;
                        }
                        subscribers.notify(valueProxy);
                    }
                },
            });
        }
        return model;
    }
    static subscribe(SubscriberType, modelObject, path, ...args) {
        let model = modelObject;
        let keys = path.split(".");
        let key = path;
        for (let i = 0, len = keys.length - 1; i < len; i++) {
            key = keys[i];
            model = model[key];
        }
        let sub = new SubscriberType(model, key, ...args);
        return sub;
    }
}
exports.DataBinding = DataBinding;
//# sourceMappingURL=data_binding.js.map