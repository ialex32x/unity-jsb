/*!
 * Vue.js v2.6.14
 * (c) 2014-2021 Evan You
 * Released under the MIT License.
 */

/**
 * Perform no operation.
 * Stubbing args to make Flow happy without leaving useless transpiled code
 * with ...rest (https://flow.org/blog/2017/05/07/Strict-Function-Call-Arity/).
 */
function noop(a, b, c) { }

function warn(m, v) { }

// can we use __proto__?
var hasProto = '__proto__' in {};

/**
 * Augment a target Object or Array by intercepting
 * the prototype chain using __proto__
 */
function protoAugment(target, src) {
    /* eslint-disable no-proto */
    target.__proto__ = src;
    /* eslint-enable no-proto */
}

/**
 * Augment a target Object or Array by defining
 * hidden properties.
 */
/* istanbul ignore next */
function copyAugment(target, src, keys) {
    for (var i = 0, l = keys.length; i < l; i++) {
        var key = keys[i];
        def(target, key, src[key]);
    }
}

/**
 * Define a property.
 */
function def(obj, key, val, enumerable?) {
    Object.defineProperty(obj, key, {
        value: val,
        enumerable: !!enumerable,
        writable: true,
        configurable: true
    });
}

/*
 * not type checking this file because flow doesn't play well with
 * dynamically accessing methods on Array prototype
 */

var arrayProto = Array.prototype;
var arrayMethods = Object.create(arrayProto);

var methodsToPatch = [
    'push',
    'pop',
    'shift',
    'unshift',
    'splice',
    'sort',
    'reverse'
];

/**
 * Intercept mutating methods and emit events
 */
methodsToPatch.forEach(function (method) {
    // cache original method
    var original = arrayProto[method];
    def(arrayMethods, method, function mutator() {
        var args = [], len = arguments.length;
        while (len--) args[len] = arguments[len];

        var result = original.apply(this, args);
        var ob = this.__ob__;
        var inserted;
        switch (method) {
            case 'push':
            case 'unshift':
                inserted = args;
                break
            case 'splice':
                inserted = args.slice(2);
                break
        }
        if (inserted) { ob.observeArray(inserted); }
        // notify change
        ob.dep.notify();
        return result
    });
});

/*  */

var arrayKeys = Object.getOwnPropertyNames(arrayMethods);

/**
 * Remove an item from an array.
 */
function remove(arr, item) {
    if (arr.length) {
        var index = arr.indexOf(item);
        if (index > -1) {
            return arr.splice(index, 1)
        }
    }
}

/*  */

var uid = 0;

/**
 * A dep is an observable that can have multiple
 * directives subscribing to it.
 */
function Dep() {
    this.id = uid++;
    this.subs = [];
};

Dep.prototype.addSub = function addSub(sub) {
    this.subs.push(sub);
};

Dep.prototype.removeSub = function removeSub(sub) {
    remove(this.subs, sub);
};

Dep.prototype.depend = function depend() {
    if (Dep.target) {
        Dep.target.addDep(this);
    }
};

Dep.prototype.notify = function notify() {
    // stabilize the subscriber list first
    var subs = this.subs.slice();

    for (var i = 0, l = subs.length; i < l; i++) {
        subs[i].update();
    }
};

// The current target watcher being evaluated.
// This is globally unique because only one watcher
// can be evaluated at a time.
Dep.target = null;
var targetStack = [];

function pushTarget(target?) {
    targetStack.push(target);
    Dep.target = target;
}

function popTarget() {
    targetStack.pop();
    Dep.target = targetStack[targetStack.length - 1];
}

/**
 * Collect dependencies on array elements when the array is touched, since
 * we cannot intercept array element access like property getters.
 */
function dependArray(value) {
    for (var e = (void 0), i = 0, l = value.length; i < l; i++) {
        e = value[i];
        e && e.__ob__ && e.__ob__.dep.depend();
        if (Array.isArray(e)) {
            dependArray(e);
        }
    }
}

/**
 * Define a reactive property on an Object.
 */
function defineReactive$$1(
    obj,
    key,
    val?,
    customSetter?,
    shallow?
) {
    var dep = new Dep();

    var property = Object.getOwnPropertyDescriptor(obj, key);
    if (property && property.configurable === false) {
        return
    }

    // cater for pre-defined getter/setters
    var getter = property && property.get;
    var setter = property && property.set;
    if ((!getter || setter) && arguments.length === 2) {
        val = obj[key];
    }

    var childOb = !shallow && observe(val);
    Object.defineProperty(obj, key, {
        enumerable: true,
        configurable: true,
        get: function reactiveGetter() {
            var value = getter ? getter.call(obj) : val;
            if (Dep.target) {
                dep.depend();
                if (childOb) {
                    childOb.dep.depend();
                    if (Array.isArray(value)) {
                        dependArray(value);
                    }
                }
            }
            return value
        },
        set: function reactiveSetter(newVal) {
            var value = getter ? getter.call(obj) : val;
            /* eslint-disable no-self-compare */
            if (newVal === value || (newVal !== newVal && value !== value)) {
                return
            }
            /* eslint-enable no-self-compare */
            if (customSetter) {
                customSetter();
            }
            // #7981: for accessor properties without setter
            if (getter && !setter) { return }
            if (setter) {
                setter.call(obj, newVal);
            } else {
                val = newVal;
            }
            childOb = !shallow && observe(newVal);
            dep.notify();
        }
    });
}

/**
 * Check whether an object has the property.
 */
var hasOwnProperty = Object.prototype.hasOwnProperty;
function hasOwn(obj, key) {
    return hasOwnProperty.call(obj, key)
}

/**
 * Quick object check - this is primarily used to tell
 * Objects from primitive values when we know the value
 * is a JSON-compliant type.
 */
function isObject(obj) {
    return obj !== null && typeof obj === 'object'
}

/**
 * In some cases we may want to disable observation inside a component's
 * update computation.
 */
var shouldObserve = true;

function toggleObserving(value) {
    shouldObserve = value;
}

/**
 * Get the raw type string of a value, e.g., [object Object].
 */
var _toString = Object.prototype.toString;

function toRawType(value) {
    return _toString.call(value).slice(8, -1)
}

/**
 * Strict object type check. Only returns true
 * for plain JavaScript objects.
 */
function isPlainObject(obj) {
    return _toString.call(obj) === '[object Object]'
}

function isRegExp(v) {
    return _toString.call(v) === '[object RegExp]'
}

/**
 * Attempt to create an observer instance for a value,
 * returns the new observer if successfully observed,
 * or the existing observer if the value already has one.
 */
function observe(value, asRootData?) {
    if (!isObject(value)) {
        return
    }
    var ob;
    if (hasOwn(value, '__ob__') && value.__ob__ instanceof Observer) {
        ob = value.__ob__;
    } else if (
        shouldObserve &&
        (Array.isArray(value) || isPlainObject(value)) &&
        Object.isExtensible(value) &&
        !value._isVue
    ) {
        ob = new Observer(value);
    }

    if (asRootData && ob) {
        ob.vmCount++;
    }

    return ob
}

/* istanbul ignore next */
function isNative(Ctor) {
    return typeof Ctor === 'function' && /native code/.test(Ctor.toString())
}

var _Set;
/* istanbul ignore if */ // $flow-disable-line
if (typeof Set !== 'undefined' && isNative(Set)) {
    // use native Set when available.
    _Set = Set;
} else {
    // a non-standard Set polyfill that only works with primitive keys.
    _Set = /*@__PURE__*/(function () {
        function Set() {
            this.set = Object.create(null);
        }
        Set.prototype.has = function has(key) {
            return this.set[key] === true
        };
        Set.prototype.add = function add(key) {
            this.set[key] = true;
        };
        Set.prototype.clear = function clear() {
            this.set = Object.create(null);
        };

        return Set;
    }());
}

/*  */

/**
 * unicode letters used for parsing html tags, component names and property paths.
 * using https://www.w3.org/TR/html53/semantics-scripting.html#potentialcustomelementname
 * skipping \u10000-\uEFFFF due to it freezing up PhantomJS
 */
var unicodeRegExp = /a-zA-Z\u00B7\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u037D\u037F-\u1FFF\u200C-\u200D\u203F-\u2040\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD/;

/**
 * Parse simple path.
 */
var bailRE = new RegExp(("[^" + (unicodeRegExp.source) + ".$_\\d]"));
function parsePath(path) {
    if (bailRE.test(path)) {
        return
    }
    var segments = path.split('.');
    return function (obj) {
        for (var i = 0; i < segments.length; i++) {
            if (!obj) { return }
            obj = obj[segments[i]];
        }
        return obj
    }
}

/*  */

var seenObjects = new _Set();

/**
 * Recursively traverse an object to evoke all converted
 * getters, so that every nested property inside the object
 * is collected as a "deep" dependency.
 */
function traverse(val) {
    _traverse(val, seenObjects);
    seenObjects.clear();
}

function _traverse(val, seen) {
    var i, keys;
    var isA = Array.isArray(val);
    if ((!isA && !isObject(val)) || Object.isFrozen(val)/* || val instanceof VNode */) {
        return
    }
    if (val.__ob__) {
        var depId = val.__ob__.dep.id;
        if (seen.has(depId)) {
            return
        }
        seen.add(depId);
    }
    if (isA) {
        i = val.length;
        while (i--) { _traverse(val[i], seen); }
    } else {
        keys = Object.keys(val);
        i = keys.length;
        while (i--) { _traverse(val[keys[i]], seen); }
    }
}

/*  */

function handleError(err, vm, info) {
    // Deactivate deps tracking while processing error handler to avoid possible infinite rendering.
    // See: https://github.com/vuejs/vuex/issues/1505
    pushTarget();
    try {
        if (vm) {
            var cur = vm;
            while ((cur = cur.$parent)) {
                var hooks = cur.$options.errorCaptured;
                if (hooks) {
                    for (var i = 0; i < hooks.length; i++) {
                        try {
                            var capture = hooks[i].call(cur, err, vm, info) === false;
                            if (capture) { return }
                        } catch (e) {
                            globalHandleError(e, cur, 'errorCaptured hook');
                        }
                    }
                }
            }
        }
        globalHandleError(err, vm, info);
    } finally {
        popTarget();
    }
}

function isDef(v) {
    return v !== undefined && v !== null
}

function isPromise(val) {
    return (
        isDef(val) &&
        typeof val.then === 'function' &&
        typeof val.catch === 'function'
    )
}

function invokeWithErrorHandling(
    handler,
    context,
    args,
    vm,
    info
) {
    var res;
    try {
        res = args ? handler.apply(context, args) : handler.call(context);
        if (res && !res._isVue && isPromise(res) && !res._handled) {
            res.catch(function (e) { return handleError(e, vm, info + " (Promise/async)"); });
            // issue #9511
            // avoid catch triggering multiple times when nested calls
            res._handled = true;
        }
    } catch (e) {
        handleError(e, vm, info);
    }
    return res
}

function globalHandleError(err, vm, info) {
    // if (config.errorHandler) {
    //     try {
    //         return config.errorHandler.call(null, err, vm, info)
    //     } catch (e) {
    //         // if the user intentionally throws the original error in the handler,
    //         // do not log it twice
    //         if (e !== err) {
    //             logError(e, null, 'config.errorHandler');
    //         }
    //     }
    // }
    logError(err, vm, info);
}

function logError(err, vm, info) {
    {
        warn(("Error in " + info + ": \"" + (err.toString()) + "\""), vm);
    }
    /* istanbul ignore else */
    if (typeof console !== 'undefined') {
        console.error(err);
    } else {
        throw err
    }
}

/**
 * Push a watcher into the watcher queue.
 * Jobs with duplicate IDs will be skipped unless it's
 * pushed when the queue is being flushed.
 */
function queueWatcher(watcher) {
    var id = watcher.id;
    if (has[id] == null) {
        has[id] = true;
        if (!flushing) {
            queue.push(watcher);
        } else {
            // if already flushing, splice the watcher based on its id
            // if already past its id, it will be run next immediately.
            var i = queue.length - 1;
            while (i > index && queue[i].id > watcher.id) {
                i--;
            }
            queue.splice(i + 1, 0, watcher);
        }
        // queue the flush
        if (!waiting) {
            waiting = true;

            // if (!config.async) {
            //     flushSchedulerQueue();
            //     return
            // }
            nextTick(flushSchedulerQueue);
        }
    }
}
var callbacks = [];
var pending = false;

function flushCallbacks() {
    pending = false;
    var copies = callbacks.slice(0);
    callbacks.length = 0;
    for (var i = 0; i < copies.length; i++) {
        copies[i]();
    }
}

// Here we have async deferring wrappers using microtasks.
// In 2.5 we used (macro) tasks (in combination with microtasks).
// However, it has subtle problems when state is changed right before repaint
// (e.g. #6813, out-in transitions).
// Also, using (macro) tasks in event handler would cause some weird behaviors
// that cannot be circumvented (e.g. #7109, #7153, #7546, #7834, #8109).
// So we now use microtasks everywhere, again.
// A major drawback of this tradeoff is that there are some scenarios
// where microtasks have too high a priority and fire in between supposedly
// sequential events (e.g. #4521, #6690, which have workarounds)
// or even between bubbling of the same event (#6566).
var timerFunc;

var p = Promise.resolve();
timerFunc = function () {
    p.then(flushCallbacks);
};

function nextTick(cb, ctx?) {
    var _resolve;
    callbacks.push(function () {
        if (cb) {
            try {
                cb.call(ctx);
            } catch (e) {
                handleError(e, ctx, 'nextTick');
            }
        } else if (_resolve) {
            _resolve(ctx);
        }
    });
    if (!pending) {
        pending = true;
        timerFunc();
    }
    // $flow-disable-line
    if (!cb && typeof Promise !== 'undefined') {
        return new Promise(function (resolve) {
            _resolve = resolve;
        })
    }
}

/*  */

var MAX_UPDATE_COUNT = 100;

var queue = [];
var activatedChildren = [];
var has = {};
var circular = {};
var waiting = false;
var flushing = false;
var index = 0;

/**
 * Reset the scheduler's state.
 */
function resetSchedulerState() {
    index = queue.length = activatedChildren.length = 0;
    has = {};
    {
        circular = {};
    }
    waiting = flushing = false;
}

// Async edge case #6566 requires saving the timestamp when event listeners are
// attached. However, calling performance.now() has a perf overhead especially
// if the page has thousands of event listeners. Instead, we take a timestamp
// every time the scheduler flushes and use that for all event listeners
// attached during that flush.
var currentFlushTimestamp = 0;

// Async edge case fix requires storing an event listener's attach timestamp.
var getNow = Date.now;

/**
 * Flush both queues and run the watchers.
 */
function flushSchedulerQueue() {
    currentFlushTimestamp = getNow();
    flushing = true;
    var watcher, id;

    // Sort queue before flush.
    // This ensures that:
    // 1. Components are updated from parent to child. (because parent is always
    //    created before the child)
    // 2. A component's user watchers are run before its render watcher (because
    //    user watchers are created before the render watcher)
    // 3. If a component is destroyed during a parent component's watcher run,
    //    its watchers can be skipped.
    queue.sort(function (a, b) { return a.id - b.id; });

    // do not cache length because more watchers might be pushed
    // as we run existing watchers
    for (index = 0; index < queue.length; index++) {
        watcher = queue[index];
        if (watcher.before) {
            watcher.before();
        }
        id = watcher.id;
        has[id] = null;
        watcher.run();
        // in dev build, check and stop circular updates.
        if (has[id] != null) {
            circular[id] = (circular[id] || 0) + 1;
            if (circular[id] > MAX_UPDATE_COUNT) {
                warn(
                    'You may have an infinite update loop ' + (
                        watcher.user
                            ? ("in watcher with expression \"" + (watcher.expression) + "\"")
                            : "in a component render function."
                    ),
                    watcher.vm
                );
                break
            }
        }
    }

    // keep copies of post queues before resetting state
    var activatedQueue = activatedChildren.slice();
    var updatedQueue = queue.slice();

    resetSchedulerState();

    // call component updated and activated hooks
    callActivatedHooks(activatedQueue);
    callUpdatedHooks(updatedQueue);

    // // devtool hook
    // /* istanbul ignore if */
    // if (devtools && config.devtools) {
    //     devtools.emit('flush');
    // }
}

function callActivatedHooks(queue) {
    for (var i = 0; i < queue.length; i++) {
        queue[i]._inactive = true;
        // activateChildComponent(queue[i], true /* true */);
    }
}

function callHook(vm, hook) {
    // #7573 disable dep collection when invoking lifecycle hooks
    pushTarget();
    var handlers = vm.$options[hook];
    var info = hook + " hook";
    if (handlers) {
        for (var i = 0, j = handlers.length; i < j; i++) {
            invokeWithErrorHandling(handlers[i], vm, null, vm, info);
        }
    }
    if (vm._hasHookEvent) {
        vm.$emit('hook:' + hook);
    }
    popTarget();
}

function callUpdatedHooks(queue) {
    var i = queue.length;
    while (i--) {
        var watcher = queue[i];
        var vm = watcher.vm;
        if (vm._watcher === watcher && vm._isMounted && !vm._isDestroyed) {
            callHook(vm, 'updated');
        }
    }
}

/*  */



var uid$2 = 0;

/**
 * A watcher parses an expression, collects dependencies,
 * and fires callback when the expression value changes.
 * This is used for both the $watch() api and directives.
 */
function Watcher(
    vm,
    expOrFn,
    cb,
    options?,
    isRenderWatcher?
) {
    this.vm = vm;
    if (isRenderWatcher) {
        vm._watcher = this;
    }
    vm._watchers.push(this);
    // options
    if (options) {
        this.deep = !!options.deep;
        this.user = !!options.user;
        this.lazy = !!options.lazy;
        this.sync = !!options.sync;
        this.before = options.before;
    } else {
        this.deep = this.user = this.lazy = this.sync = false;
    }
    this.cb = cb;
    this.id = ++uid$2; // uid for batching
    this.active = true;
    this.dirty = this.lazy; // for lazy watchers
    this.deps = [];
    this.newDeps = [];
    this.depIds = new _Set();
    this.newDepIds = new _Set();
    this.expression = expOrFn.toString();
    // parse expression for getter
    if (typeof expOrFn === 'function') {
        this.getter = expOrFn;
    } else {
        this.getter = parsePath(expOrFn);
        if (!this.getter) {
            this.getter = noop;
            warn(
                "Failed watching path: \"" + expOrFn + "\" " +
                'Watcher only accepts simple dot-delimited paths. ' +
                'For full control, use a function instead.',
                vm
            );
        }
    }
    this.value = this.lazy
        ? undefined
        : this.get();
};

/**
 * Evaluate the getter, and re-collect dependencies.
 */
Watcher.prototype.get = function get() {
    pushTarget(this);
    var value;
    var vm = this.vm;
    try {
        value = this.getter.call(vm, vm);
    } catch (e) {
        if (this.user) {
            handleError(e, vm, ("getter for watcher \"" + (this.expression) + "\""));
        } else {
            throw e
        }
    } finally {
        // "touch" every property so they are all tracked as
        // dependencies for deep watching
        if (this.deep) {
            traverse(value);
        }
        popTarget();
        this.cleanupDeps();
    }
    return value
};

/**
 * Add a dependency to this directive.
 */
Watcher.prototype.addDep = function addDep(dep) {
    var id = dep.id;
    if (!this.newDepIds.has(id)) {
        this.newDepIds.add(id);
        this.newDeps.push(dep);
        if (!this.depIds.has(id)) {
            dep.addSub(this);
        }
    }
};

/**
 * Clean up for dependency collection.
 */
Watcher.prototype.cleanupDeps = function cleanupDeps() {
    var i = this.deps.length;
    while (i--) {
        var dep = this.deps[i];
        if (!this.newDepIds.has(dep.id)) {
            dep.removeSub(this);
        }
    }
    var tmp = this.depIds;
    this.depIds = this.newDepIds;
    this.newDepIds = tmp;
    this.newDepIds.clear();
    tmp = this.deps;
    this.deps = this.newDeps;
    this.newDeps = tmp;
    this.newDeps.length = 0;
};

/**
 * Subscriber interface.
 * Will be called when a dependency changes.
 */
Watcher.prototype.update = function update() {
    /* istanbul ignore else */
    if (this.lazy) {
        this.dirty = true;
    } else if (this.sync) {
        this.run();
    } else {
        queueWatcher(this);
    }
};

/**
 * Scheduler job interface.
 * Will be called by the scheduler.
 */
Watcher.prototype.run = function run() {
    if (this.active) {
        var value = this.get();
        if (
            value !== this.value ||
            // Deep watchers and watchers on Object/Arrays should fire even
            // when the value is the same, because the value may
            // have mutated.
            isObject(value) ||
            this.deep
        ) {
            // set new value
            var oldValue = this.value;
            this.value = value;
            if (this.user) {
                var info = "callback for watcher \"" + (this.expression) + "\"";
                invokeWithErrorHandling(this.cb, this.vm, [value, oldValue], this.vm, info);
            } else {
                this.cb.call(this.vm, value, oldValue);
            }
        }
    }
};

/**
 * Evaluate the value of the watcher.
 * This only gets called for lazy watchers.
 */
Watcher.prototype.evaluate = function evaluate() {
    this.value = this.get();
    this.dirty = false;
};

/**
 * Depend on all deps collected by this watcher.
 */
Watcher.prototype.depend = function depend() {
    var i = this.deps.length;
    while (i--) {
        this.deps[i].depend();
    }
};

/**
 * Remove self from all dependencies' subscriber list.
 */
Watcher.prototype.teardown = function teardown() {
    if (this.active) {
        // remove self from vm's watcher list
        // this is a somewhat expensive operation so we skip it
        // if the vm is being destroyed.
        if (!this.vm._isBeingDestroyed) {
            remove(this.vm._watchers, this);
        }
        var i = this.deps.length;
        while (i--) {
            this.deps[i].removeSub(this);
        }
        this.active = false;
    }
};

/**
 * Observer class that is attached to each observed
 * object. Once attached, the observer converts the target
 * object's property keys into getter/setters that
 * collect dependencies and dispatch updates.
 */
var Observer = function Observer(value) {
    this.value = value;
    this.dep = new Dep();
    this.vmCount = 0;
    def(value, '__ob__', this);
    if (Array.isArray(value)) {
        if (hasProto) {
            protoAugment(value, arrayMethods);
        } else {
            copyAugment(value, arrayMethods, arrayKeys);
        }
        this.observeArray(value);
    } else {
        this.walk(value);
    }
};

/**
 * Walk through all properties and convert them into
 * getter/setters. This method should only be called when
 * value type is Object.
 */
Observer.prototype.walk = function walk(obj) {
    var keys = Object.keys(obj);
    for (var i = 0; i < keys.length; i++) {
        defineReactive$$1(obj, keys[i]);
    }
};

/**
 * Observe a list of Array items.
 */
Observer.prototype.observeArray = function observeArray(items) {
    for (var i = 0, l = items.length; i < l; i++) {
        observe(items[i]);
    }
};

var validDivisionCharRE = /[\w).+\-_$\]]/;

function parseFilters(exp) {
    var inSingle = false;
    var inDouble = false;
    var inTemplateString = false;
    var inRegex = false;
    var curly = 0;
    var square = 0;
    var paren = 0;
    var lastFilterIndex = 0;
    var c, prev, i, expression, filters;

    for (i = 0; i < exp.length; i++) {
        prev = c;
        c = exp.charCodeAt(i);
        if (inSingle) {
            if (c === 0x27 && prev !== 0x5C) { inSingle = false; }
        } else if (inDouble) {
            if (c === 0x22 && prev !== 0x5C) { inDouble = false; }
        } else if (inTemplateString) {
            if (c === 0x60 && prev !== 0x5C) { inTemplateString = false; }
        } else if (inRegex) {
            if (c === 0x2f && prev !== 0x5C) { inRegex = false; }
        } else if (
            c === 0x7C && // pipe
            exp.charCodeAt(i + 1) !== 0x7C &&
            exp.charCodeAt(i - 1) !== 0x7C &&
            !curly && !square && !paren
        ) {
            if (expression === undefined) {
                // first filter, end of expression
                lastFilterIndex = i + 1;
                expression = exp.slice(0, i).trim();
            } else {
                pushFilter();
            }
        } else {
            switch (c) {
                case 0x22: inDouble = true; break         // "
                case 0x27: inSingle = true; break         // '
                case 0x60: inTemplateString = true; break // `
                case 0x28: paren++; break                 // (
                case 0x29: paren--; break                 // )
                case 0x5B: square++; break                // [
                case 0x5D: square--; break                // ]
                case 0x7B: curly++; break                 // {
                case 0x7D: curly--; break                 // }
            }
            if (c === 0x2f) { // /
                var j = i - 1;
                var p = (void 0);
                // find first non-whitespace prev char
                for (; j >= 0; j--) {
                    p = exp.charAt(j);
                    if (p !== ' ') { break }
                }
                if (!p || !validDivisionCharRE.test(p)) {
                    inRegex = true;
                }
            }
        }
    }

    if (expression === undefined) {
        expression = exp.slice(0, i).trim();
    } else if (lastFilterIndex !== 0) {
        pushFilter();
    }

    function pushFilter() {
        (filters || (filters = [])).push(exp.slice(lastFilterIndex, i).trim());
        lastFilterIndex = i + 1;
    }

    if (filters) {
        for (i = 0; i < filters.length; i++) {
            expression = wrapFilter(expression, filters[i]);
        }
    }

    return expression
}

function wrapFilter(exp, filter) {
    var i = filter.indexOf('(');
    if (i < 0) {
        // _f: resolveFilter
        return ("_f(\"" + filter + "\")(" + exp + ")")
    } else {
        var name = filter.slice(0, i);
        var args = filter.slice(i + 1);
        return ("_f(\"" + name + "\")(" + exp + (args !== ')' ? ',' + args : args))
    }
}

var defaultTagRE = /\{\{((?:.|\r?\n)+?)\}\}/g;
var regexEscapeRE = /[-.*+?^${}()|[\]\/\\]/g;

/**
 * Create a cached version of a pure function.
 */
function cached(fn) {
    var cache = Object.create(null);
    return (function cachedFn(str) {
        var hit = cache[str];
        return hit || (cache[str] = fn(str))
    })
}

var buildRegex = cached(function (delimiters) {
    var open = delimiters[0].replace(regexEscapeRE, '\\$&');
    var close = delimiters[1].replace(regexEscapeRE, '\\$&');
    return new RegExp(open + '((?:.|\\n)+?)' + close, 'g')
});



function parseText(
    text,
    delimiters
) {
    var tagRE = delimiters ? buildRegex(delimiters) : defaultTagRE;
    if (!tagRE.test(text)) {
        console.error(tagRE, text);
        return
    }
    var tokens = [];
    var rawTokens = [];
    var lastIndex = tagRE.lastIndex = 0;
    var match, index, tokenValue;
    while ((match = tagRE.exec(text))) {
        index = match.index;
        // push text token
        if (index > lastIndex) {
            rawTokens.push(tokenValue = text.slice(lastIndex, index));
            tokens.push(JSON.stringify(tokenValue));
        }
        // tag token
        var exp = parseFilters(match[1].trim());
        tokens.push(("_s(" + exp + ")"));
        rawTokens.push({ '@binding': exp });
        lastIndex = index + match[0].length;
    }
    if (lastIndex < text.length) {
        rawTokens.push(tokenValue = text.slice(lastIndex));
        tokens.push(JSON.stringify(tokenValue));
    }
    return {
        expression: tokens.join('+'),
        tokens: rawTokens
    }
}

export interface IViewModelWatcher {
    readonly value: string,
    readonly dirty: boolean,
    readonly expression: string,
    readonly id: number;
    cb: (value, oldValue) => void,

    evaluate: () => void,
    teardown: () => void,
}

export class ViewModel {
    static create<T>(data: T): T {
        let vm = new ViewModel(data);
        return <T><any>vm;
    }

    constructor(data: any) {
        Object.defineProperty(this, "_watchers", {
            value: [],
            enumerable: false,
            writable: true,
            configurable: false,
        });

        Object.defineProperty(this, "_isVue", {
            value: true,
            enumerable: false,
            writable: false,
            configurable: false,
        });

        let keys = Object.keys(data);
        let i = keys.length;

        while (i--) {
            let key = keys[i];

            Object.defineProperty(this, key, {
                enumerable: true,
                configurable: true,
                get: function () {
                    return data[key];
                },
                set: function (val) {
                    data[key] = val;
                },
            });
        }
        
        observe(data, true);
    }

    static $toString(v) {
        return new String(v);
    }

    // 临时代码
    static flush() {
        flushSchedulerQueue();
    }

    static expression(vm: any, expression: string, cb?: (value, oldValue) => void): IViewModelWatcher {
        if (vm instanceof ViewModel) {
            let exp = parseText(expression, null);
            let fn = function () { return eval("(function (_s) { return " + exp.expression + "; })").call(this, ViewModel.$toString); };
            return new Watcher(vm, fn, cb);
        }
    }

    static field(vm: any, path: string, cb?: (value, oldValue) => void): IViewModelWatcher {
        if (vm instanceof ViewModel) {
            return new Watcher(vm, path, cb);
        }
    }
}
