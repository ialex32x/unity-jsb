"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CreateJSArrayProxy = exports.GetUnderlyingArray = void 0;
const jsb = require("jsb");
const $GetLength = jsb.ArrayUtils.GetLength;
const $SetValue = jsb.ArrayUtils.SetValue;
const $GetValue = jsb.ArrayUtils.GetValue;
const $RemoveAt = jsb.ArrayUtils.RemoveAt;
const $Insert = jsb.ArrayUtils.Insert;
const UnderlyingValueAccess = Symbol.for("Underlying Native Array");
function GetUnderlyingArray(p) {
    return p[UnderlyingValueAccess];
}
exports.GetUnderlyingArray = GetUnderlyingArray;
function CreateJSArrayProxy(target) {
    let _members = {
        /**
         * Returns the value of the first element in the array where predicate is true, and undefined
         * otherwise.
         * @param predicate find calls predicate once for each element of the array, in ascending
         * order, until it finds one where predicate returns true. If such an element is found, find
         * immediately returns that element value. Otherwise, find returns undefined.
         * @param thisArg If provided, it will be used as the this value for each invocation of
         * predicate. If it is not provided, undefined is used instead.
         */
        // find<S extends T>(predicate: (this: void, value: T, index: number, obj: T[]) => value is S, thisArg?: any): S | undefined;
        "find": function (predicate, thisArg) {
            let copy = new Array(...this);
            for (let i = 0, n = copy.length; i < n; ++i) {
                let v = copy[i];
                if (Boolean(predicate.call(thisArg, v, i, this))) {
                    return v;
                }
            }
            // return new Array(...this).find(predicate, thisArg);
        },
        /**
         * Returns the index of the first element in the array where predicate is true, and -1
         * otherwise.
         * @param predicate find calls predicate once for each element of the array, in ascending
         * order, until it finds one where predicate returns true. If such an element is found,
         * findIndex immediately returns that element index. Otherwise, findIndex returns -1.
         * @param thisArg If provided, it will be used as the this value for each invocation of
         * predicate. If it is not provided, undefined is used instead.
         */
        "findIndex": function (predicate, thisArg) {
            let copy = new Array(...this);
            for (let i = 0, n = copy.length; i < n; ++i) {
                let v = copy[i];
                if (Boolean(predicate.call(thisArg, v, i, this))) {
                    return i;
                }
            }
            return -1;
            // return new Array(...this).findIndex(predicate, thisArg);
        },
        /**
         * Changes all array elements from `start` to `end` index to a static `value` and returns the modified array
         * @param value value to fill array section with
         * @param start index to start filling the array at. If start is negative, it is treated as
         * length+start where length is the length of the array.
         * @param end index to stop filling the array at. If end is negative, it is treated as
         * length+end.
         */
        fill(value, start, end) {
            let n = $GetLength(target);
            if (typeof start === "undefined") {
                start = 0;
            }
            else if (start < 0) {
                start = n + start;
            }
            if (typeof end === "undefined") {
                end = n;
            }
            else if (end < 0) {
                end = n + end;
            }
            for (let i = start; i < end; ++i) {
                $SetValue(target, i, value);
            }
            return this;
        },
        /**
         * Returns the this object after copying a section of the array identified by start and end
         * to the same array starting at position target
         * @param target If target is negative, it is treated as length+target where length is the
         * length of the array.
         * @param start If start is negative, it is treated as length+start. If end is negative, it
         * is treated as length+end.
         * @param end If not specified, length of the this object is used as its default value.
         */
        "copyWithin": function (target_, start, end) {
            let c = new Array(...this);
            c.copyWithin(target_, start, end);
            for (let i = 0, n = c.length; i < n; ++i) {
                $SetValue(target, i, c[i]);
            }
            return this;
        },
        /** es2015.iterable */
        [Symbol.iterator]: function () {
            let i = 0;
            return {
                next: function () {
                    let currentIndex = i++;
                    return currentIndex < $GetLength(target) ? {
                        value: $GetValue(target, currentIndex),
                        done: false,
                    } : { done: true };
                },
            };
        },
        /** es2015.iterable */
        "entries": function () {
            return {
                [Symbol.iterator]: function () {
                    let i = 0;
                    return {
                        next: function () {
                            let currentIndex = i++;
                            return currentIndex < $GetLength(target) ? {
                                value: [currentIndex, $GetValue(target, currentIndex)],
                                done: false,
                            } : { done: true };
                        },
                    };
                }
            };
        },
        /** es2015.iterable */
        "keys": function () {
            return {
                [Symbol.iterator]: function () {
                    let i = 0;
                    return {
                        next: function () {
                            let currentIndex = i++;
                            return currentIndex < $GetLength(target) ? {
                                value: currentIndex,
                                done: false,
                            } : { done: true };
                        },
                    };
                }
            };
        },
        /** es2015.iterable */
        "values": function () {
            return {
                [Symbol.iterator]: function () {
                    let i = 0;
                    return {
                        next: function () {
                            let currentIndex = i++;
                            return currentIndex < $GetLength(target) ? {
                                value: $GetValue(target, currentIndex),
                                done: false,
                            } : { done: true };
                        },
                    };
                }
            };
        },
        /**
         * es5
         * Returns a string representation of an array.
         */
        "toString": function () {
            return `ArrayWrapper(${this.length})`;
        },
        /**
         * es5
         * Returns a string representation of an array. The elements are converted to string using their toLocaleString methods.
         */
        "toLocaleString": function () {
            return `ArrayWrapper(${this.length})`;
        },
        /**
         * es5
         * Removes the last element from an array and returns it.
         * If the array is empty, undefined is returned and the array is not modified.
         */
        "pop": function () {
            if ($GetLength(target) > 0) {
                let v = $GetValue(target, $GetLength(target) - 1);
                $RemoveAt(target, $GetLength(target) - 1);
                return v;
            }
        },
        /**
         * es5
         * Appends new elements to the end of an array, and returns the new length of the array.
         * @param items New elements to add to the array.
         */
        "push": function (...items) {
            let n = $GetLength(target);
            for (let i = 0; i < items.length; i++) {
                console.log("set value", i, n + 1);
                $SetValue(target, n + i, items[i]);
            }
        },
        /**
         * es5
         * Combines two or more arrays.
         * This method returns a new array without modifying any existing arrays.
         * @param items Additional arrays and/or items to add to the end of the array.
         */
        "concat": function (...items) {
            let r = new Array(...this);
            for (let ii = 0, il = items.length; ii < il; ++ii) {
                let e = items[ii];
                if (typeof e === "object" && e instanceof Array) {
                    for (let si = 0, sl = e.length; si < sl; ++si) {
                        r.push(e[si]);
                    }
                }
                else {
                    r.push(e);
                }
            }
            return r;
        },
        /**
         * es5
         * Adds all the elements of an array into a string, separated by the specified separator string.
         * @param separator A string used to separate one element of the array from the next in the resulting string. If omitted, the array elements are separated with a comma.
         */
        "join": function (separator) {
            if (typeof separator === "undefined") {
                separator = ",";
            }
            let r = "";
            for (let i = 0, n = $GetLength(target); i < n; ++i) {
                if (i != n - 1) {
                    r += $GetValue(target, i) + separator;
                }
                else {
                    r += $GetValue(target, i);
                }
            }
            return r;
        },
        /**
         * es5
         * Reverses the elements in an array in place.
         * This method mutates the array and returns a reference to the same array.
         */
        "reverse": function () {
            let n = $GetLength(target);
            let m = Math.floor(n / 2);
            for (let i = 0; i < m; ++i) {
                let i2 = n - i - 1;
                let t1 = $GetValue(target, i2);
                let t2 = $GetValue(target, i);
                $SetValue(target, i, t1);
                $SetValue(target, i2, t2);
            }
            return this;
        },
        /**
         * es5
         * Removes the first element from an array and returns it.
         * If the array is empty, undefined is returned and the array is not modified.
         */
        "shift": function () {
            if ($GetLength(target) > 0) {
                let f = $GetValue(target, 0);
                $RemoveAt(target, 0);
                return f;
            }
        },
        /**
         * es5
         * Returns a copy of a section of an array.
         * For both start and end, a negative index can be used to indicate an offset from the end of the array.
         * For example, -2 refers to the second to last element of the array.
         * @param start The beginning index of the specified portion of the array.
         * If start is undefined, then the slice begins at index 0.
         * @param end The end index of the specified portion of the array. This is exclusive of the element at the index 'end'.
         * If end is undefined, then the slice extends to the end of the array.
         */
        "slice": function (start, end) {
            let n = $GetLength(target);
            let r = [];
            if (typeof start === "undefined") {
                start = 0;
            }
            else if (start < 0) {
                start = n + start;
            }
            if (typeof end === "undefined") {
                end = 0;
            }
            else if (end < 0) {
                end = n + end;
            }
            if (start < end) {
                for (let i = start; i < end; ++i) {
                    r.push($GetValue(target, i));
                }
            }
            return r;
        },
        /**
         * es5
         * Sorts an array in place.
         * This method mutates the array and returns a reference to the same array.
         * @param compareFn Function used to determine the order of the elements. It is expected to return
         * a negative value if the first argument is less than the second argument, zero if they're equal, and a positive
         * value otherwise. If omitted, the elements are sorted in ascending, ASCII character order.
         * ```ts
         * [11,2,22,1].sort((a, b) => a - b)
         * ```
         */
        "sort": function (compareFn) {
            let n = $GetLength(target);
            if (n > 1) {
                let r = new Array(...this).sort(compareFn);
                for (let i = 0; i < n; ++i) {
                    $SetValue(target, i, r[i]);
                }
            }
            return this;
        },
        /**
         * es5
         * Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
         * @param start The zero-based location in the array from which to start removing elements.
         * @param deleteCount The number of elements to remove.
         * @param items Elements to insert into the array in place of the deleted elements.
         * @returns An array containing the elements that were deleted.
         */
        "splice": function (start, deleteCount, ...items) {
            let len = $GetLength(target);
            let res = [];
            if (start < len) {
                let m = len - start;
                if (typeof deleteCount === "number" && m > deleteCount) {
                    m = deleteCount;
                }
                for (let i = 0; i < m; ++i) {
                    res.push($GetValue(target, start));
                    $RemoveAt(target, start);
                }
                for (let i = 0, c = items.length; i < c; ++i) {
                    $Insert(target, start, items[c - i - 1]);
                }
            }
            else {
                for (let i = 0, c = items.length; i < c; ++i) {
                    $Insert(target, len, items[c - i - 1]);
                }
            }
            return res;
        },
        /**
         * es5
         * Inserts new elements at the start of an array, and returns the new length of the array.
         * @param items Elements to insert at the start of the array.
         */
        "unshift": function (...items) {
            for (let i = 0, n = items.length; i < n; ++i) {
                $Insert(target, 0, items[n - i - 1]);
            }
            return $GetLength(target);
        },
        /**
         * es5
         * Returns the index of the first occurrence of a value in an array, or -1 if it is not present.
         * @param searchElement The value to locate in the array.
         * @param fromIndex The array index at which to begin the search. If fromIndex is omitted, the search starts at index 0.
         */
        "indexOf": function (searchElement, fromIndex) {
            if (typeof fromIndex !== "number") {
                fromIndex = 0;
            }
            else {
                fromIndex = fromIndex <= 0 ? 0 : Math.floor(fromIndex);
            }
            for (let i = fromIndex, n = $GetLength(target); i < n; ++i) {
                if ($GetValue(target, i) == searchElement) {
                    return i;
                }
            }
            return -1;
        },
        /**
         * es5
         * Returns the index of the last occurrence of a specified value in an array, or -1 if it is not present.
         * @param searchElement The value to locate in the array.
         * @param fromIndex The array index at which to begin searching backward. If fromIndex is omitted, the search starts at the last index in the array.
         */
        "lastIndexOf": function (searchElement, fromIndex) {
            if (typeof fromIndex !== "number") {
                fromIndex = $GetLength(target) - 1;
            }
            else {
                if (fromIndex < 0) {
                    fromIndex = $GetLength(target) + fromIndex;
                }
            }
            for (let i = fromIndex; i >= 0; --i) {
                if ($GetValue(target, i) == searchElement) {
                    return i;
                }
            }
            return -1;
        },
        /**
         * es5
         * Determines whether all the members of an array satisfy the specified test.
         * @param predicate A function that accepts up to three arguments. The every method calls
         * the predicate function for each element in the array until the predicate returns a value
         * which is coercible to the Boolean value false, or until the end of the array.
         * @param thisArg An object to which the this keyword can refer in the predicate function.
         * If thisArg is omitted, undefined is used as the this value.
         */
        "every": function (predicate, thisArg) {
            for (let i = 0, n = $GetLength(target); i < n; ++i) {
                let item = $GetValue(target, i);
                if (!Boolean(predicate.call(thisArg, item, i, this))) {
                    return false;
                }
            }
            return true;
        },
        /**
         * es5
         * Determines whether the specified callback function returns true for any element of an array.
         * @param predicate A function that accepts up to three arguments. The some method calls
         * the predicate function for each element in the array until the predicate returns a value
         * which is coercible to the Boolean value true, or until the end of the array.
         * @param thisArg An object to which the this keyword can refer in the predicate function.
         * If thisArg is omitted, undefined is used as the this value.
         */
        "some": function (predicate, thisArg) {
            for (let i = 0, n = $GetLength(target); i < n; ++i) {
                let item = $GetValue(target, i);
                if (Boolean(predicate.call(thisArg, item, i, this))) {
                    return true;
                }
            }
            return false;
        },
        /**
         * es5
         * Performs the specified action for each element in an array.
         * @param callbackfn  A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array.
         * @param thisArg  An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.
         */
        "forEach": function (callbackfn, thisArg) {
            if (typeof thisArg !== "undefined") {
                for (let i = 0, n = $GetLength(target); i < n; i++) {
                    callbackfn.call(thisArg, $GetValue(target, i), i, this);
                }
            }
            else {
                for (let i = 0, n = $GetLength(target); i < n; i++) {
                    callbackfn($GetValue(target, i), i, this);
                }
            }
        },
        /**
         * es5
         * Calls a defined callback function on each element of an array, and returns an array that contains the results.
         * @param callbackfn A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
         * @param thisArg An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.
         */
        map(callbackfn, thisArg) {
            let n = $GetLength(target);
            let r = new Array(n);
            for (let i = 0; i < n; i++) {
                let e = callbackfn.call(thisArg, $GetValue(target, i), i, this);
                r.push(e);
            }
            return r;
        },
        /**
         * es5
         * Returns the elements of an array that meet the condition specified in a callback function.
         * @param predicate A function that accepts up to three arguments. The filter method calls the predicate function one time for each element in the array.
         * @param thisArg An object to which the this keyword can refer in the predicate function. If thisArg is omitted, undefined is used as the this value.
         */
        "filter": function (predicate, thisArg) {
            let res = [];
            for (let i = 0, n = $GetLength(target); i < n; ++i) {
                let item = $GetValue(target, i);
                if (Boolean(predicate.call(thisArg, item, i, this))) {
                    res.push(item);
                }
            }
            return res;
        },
        /**
         * es5
         * Calls the specified callback function for all the elements in an array. The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
         * @param callbackfn A function that accepts up to four arguments. The reduce method calls the callbackfn function one time for each element in the array.
         * @param initialValue If initialValue is specified, it is used as the initial value to start the accumulation. The first call to the callbackfn function provides this value as an argument instead of an array value.
         */
        "reduce": function (callbackfn, initialValue) {
            let n = $GetLength(target);
            if (n > 0) {
                let previousValue, currentValue;
                if (typeof initialValue !== "undefined") {
                    previousValue = initialValue;
                    for (let i = 0; i < n; ++i) {
                        currentValue = $GetValue(target, i);
                        previousValue = callbackfn(previousValue, currentValue, i, this);
                    }
                    return previousValue;
                }
                else {
                    previousValue = $GetValue(target, 0);
                    for (let i = 1; i < n; ++i) {
                        currentValue = $GetValue(target, i);
                        previousValue = callbackfn(previousValue, currentValue, i, this);
                    }
                    return previousValue;
                }
            }
            return initialValue;
        },
        /**
         * es5
         * Calls the specified callback function for all the elements in an array, in descending order. The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
         * @param callbackfn A function that accepts up to four arguments. The reduceRight method calls the callbackfn function one time for each element in the array.
         * @param initialValue If initialValue is specified, it is used as the initial value to start the accumulation. The first call to the callbackfn function provides this value as an argument instead of an array value.
         */
        "reduceRight": function (callbackfn, initialValue) {
            let n = $GetLength(target);
            if (n > 0) {
                let previousValue, currentValue;
                if (typeof initialValue !== "undefined") {
                    previousValue = initialValue;
                    for (let i = n - 1; i >= 0; --i) {
                        currentValue = $GetValue(target, i);
                        previousValue = callbackfn(previousValue, currentValue, i, this);
                    }
                    return previousValue;
                }
                else {
                    previousValue = $GetValue(target, n - 1);
                    for (let i = n - 2; i >= 0; --i) {
                        currentValue = $GetValue(target, i);
                        previousValue = callbackfn(previousValue, currentValue, i, this);
                    }
                    return previousValue;
                }
            }
            return initialValue;
        },
    };
    let _getter = {
        [UnderlyingValueAccess]: function () {
            return target;
        },
        "length": function () {
            return $GetLength(target);
        },
    };
    let _setter = {
        "length": function (value) {
            throw new Error("unsupported");
        },
    };
    return new Proxy(target, {
        get(target, p, receiver) {
            if (typeof p === "string" && p.length > 0) {
                let c = p.charCodeAt(0);
                if (c >= 48 && c <= 57) {
                    let index = Number.parseInt(p);
                    return $GetValue(target, index);
                }
            }
            let mf = Object.getOwnPropertyDescriptor(_members, p) && _members[p];
            if (typeof mf !== "undefined") {
                return mf;
            }
            let mp = Object.getOwnPropertyDescriptor(_getter, p) && _getter[p];
            if (typeof mp !== "undefined") {
                return mp();
            }
            console.warn("unknown property", p);
        },
        set(target, p, value, receiver) {
            if (typeof p === "string" && p.length > 0) {
                let c = p.charCodeAt(0);
                if (c >= 48 && c <= 57) {
                    let index = Number.parseInt(p);
                    $SetValue(target, index, value);
                    return true;
                }
            }
            let mp = Object.getOwnPropertyDescriptor(_setter, p) && _setter[p];
            if (typeof mp !== "undefined") {
                mp(value);
                return true;
            }
            return false;
        },
        // getPrototypeOf(target: MyArray): object {
        //     return Array.prototype;
        // },
        ownKeys(target) {
            return new Proxy({}, {
                get(_1, p) {
                    if (typeof p === "string") {
                        if (p === "length") {
                            return $GetLength(target);
                        }
                    }
                    return p;
                },
            });
        },
        getOwnPropertyDescriptor(target, p) {
            return { enumerable: true, configurable: true, value: this[p] };
        },
    });
}
exports.CreateJSArrayProxy = CreateJSArrayProxy;
//# sourceMappingURL=array_proxy.js.map