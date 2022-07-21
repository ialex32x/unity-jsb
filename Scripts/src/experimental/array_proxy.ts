// const underlyingValue = Symbol.for("Underlying Native Array");

// function make_array(target: UnderlyingNativeArray) {
//     let _members = {
//         /**
//          * Returns the value of the first element in the array where predicate is true, and undefined
//          * otherwise.
//          * @param predicate find calls predicate once for each element of the array, in ascending
//          * order, until it finds one where predicate returns true. If such an element is found, find
//          * immediately returns that element value. Otherwise, find returns undefined.
//          * @param thisArg If provided, it will be used as the this value for each invocation of
//          * predicate. If it is not provided, undefined is used instead.
//          */
//         // find<S extends T>(predicate: (this: void, value: T, index: number, obj: T[]) => value is S, thisArg?: any): S | undefined;
//         "find": function (predicate: (value: any, index: number, obj: any[]) => unknown, thisArg?: any): any | undefined {
//             let copy = new Array(...this);
//             for (let i = 0, n = copy.length; i < n; ++i) {
//                 let v = copy[i];
//                 if (Boolean(predicate.call(thisArg, v, i, this))) {
//                     return v;
//                 }
//             }
//             // return new Array(...this).find(predicate, thisArg);
//         },

//         /**
//          * Returns the index of the first element in the array where predicate is true, and -1
//          * otherwise.
//          * @param predicate find calls predicate once for each element of the array, in ascending
//          * order, until it finds one where predicate returns true. If such an element is found,
//          * findIndex immediately returns that element index. Otherwise, findIndex returns -1.
//          * @param thisArg If provided, it will be used as the this value for each invocation of
//          * predicate. If it is not provided, undefined is used instead.
//          */
//         "findIndex": function (predicate: (value: any, index: number, obj: any[]) => unknown, thisArg?: any): number {
//             let copy = new Array(...this);
//             for (let i = 0, n = copy.length; i < n; ++i) {
//                 let v = copy[i];
//                 if (Boolean(predicate.call(thisArg, v, i, this))) {
//                     return i;
//                 }
//             }
//             return -1;
//             // return new Array(...this).findIndex(predicate, thisArg);
//         },

//         /**
//          * Changes all array elements from `start` to `end` index to a static `value` and returns the modified array
//          * @param value value to fill array section with
//          * @param start index to start filling the array at. If start is negative, it is treated as
//          * length+start where length is the length of the array.
//          * @param end index to stop filling the array at. If end is negative, it is treated as
//          * length+end.
//          */
//         fill(value: any, start?: number, end?: number): any {
//             let n = target.count;
//             if (typeof start === "undefined") {
//                 start = 0;
//             } else if (start < 0) {
//                 start = n + start;
//             }
//             if (typeof end === "undefined") {
//                 end = n;
//             } else if (end < 0) {
//                 end = n + end;
//             }
//             for (let i = start; i < end; ++i) {
//                 target.setValue(i, value);
//             }
//             return this;
//         },

//         /**
//          * Returns the this object after copying a section of the array identified by start and end
//          * to the same array starting at position target
//          * @param target If target is negative, it is treated as length+target where length is the
//          * length of the array.
//          * @param start If start is negative, it is treated as length+start. If end is negative, it
//          * is treated as length+end.
//          * @param end If not specified, length of the this object is used as its default value.
//          */
//         "copyWithin": function (target_: number, start: number, end?: number): any {
//             let c = new Array(...this);
//             c.copyWithin(target_, start, end);
//             for (let i = 0, n = c.length; i < n; ++i) {
//                 target.setValue(i, c[i]);
//             }
//             return this;
//         },

//         /** es2015.iterable */
//         [Symbol.iterator]: function () {
//             let i = 0;

//             return {
//                 next: function () {
//                     let currentIndex = i++;
//                     return currentIndex < target.count ? {
//                         value: target.getValue(currentIndex),
//                         done: false,
//                     } : { done: true };
//                 },
//             }
//         },

//         /** es2015.iterable */
//         "entries": function () {
//             return {
//                 [Symbol.iterator]: function () {
//                     let i = 0;
//                     return {
//                         next: function () {
//                             let currentIndex = i++;
//                             return currentIndex < target.count ? {
//                                 value: [currentIndex, target.getValue(currentIndex)],
//                                 done: false,
//                             } : { done: true };
//                         },
//                     }
//                 }
//             }
//         },

//         /** es2015.iterable */
//         "keys": function () {
//             return {
//                 [Symbol.iterator]: function () {
//                     let i = 0;
//                     return {
//                         next: function () {
//                             let currentIndex = i++;
//                             return currentIndex < target.count ? {
//                                 value: currentIndex,
//                                 done: false,
//                             } : { done: true };
//                         },
//                     }
//                 }
//             }
//         },

//         /** es2015.iterable */
//         "values": function () {
//             return {
//                 [Symbol.iterator]: function () {
//                     let i = 0;
//                     return {
//                         next: function () {
//                             let currentIndex = i++;
//                             return currentIndex < target.count ? {
//                                 value: target.getValue(currentIndex),
//                                 done: false,
//                             } : { done: true };
//                         },
//                     }
//                 }
//             }
//         },

//         /**
//          * es5
//          * Returns a string representation of an array.
//          */
//         "toString": function () {
//             return `ArrayWrapper(${this.length})`;
//         },
//         /**
//          * es5
//          * Returns a string representation of an array. The elements are converted to string using their toLocaleString methods.
//          */
//         "toLocaleString": function () {
//             return `ArrayWrapper(${this.length})`;
//         },
//         /**
//          * es5
//          * Removes the last element from an array and returns it.
//          * If the array is empty, undefined is returned and the array is not modified.
//          */
//         "pop": function () {
//             if (target.count > 0) {
//                 let v = target.getValue(target.count - 1);
//                 target.removeAt(target.count - 1);
//                 return v;
//             }
//         },
//         /**
//          * es5
//          * Appends new elements to the end of an array, and returns the new length of the array.
//          * @param items New elements to add to the array.
//          */
//         "push": function (...items: any[]) {
//             for (let i = 0; i < items.length; i++) {
//                 target.setValue(target.count, items[i]);
//             }
//         },
//         /**
//          * es5
//          * Combines two or more arrays.
//          * This method returns a new array without modifying any existing arrays.
//          * @param items Additional arrays and/or items to add to the end of the array.
//          */
//         "concat": function (...items: (any | ConcatArray<any>)[]): any[] {
//             let r = new Array(...this);
//             for (let ii = 0, il = items.length; ii < il; ++ii) {
//                 let e = items[ii];
//                 if (typeof e === "object" && e instanceof Array) {
//                     for (let si = 0, sl = e.length; si < sl; ++si) {
//                         r.push(e[si]);
//                     }
//                 } else {
//                     r.push(e);
//                 }
//             }
//             return r;
//         },
//         /**
//          * es5
//          * Adds all the elements of an array into a string, separated by the specified separator string.
//          * @param separator A string used to separate one element of the array from the next in the resulting string. If omitted, the array elements are separated with a comma.
//          */
//         "join": function (separator?: string): string {
//             if (typeof separator === "undefined") {
//                 separator = ",";
//             }

//             let r = "";
//             for (let i = 0, n = target.count; i < n; ++i) {
//                 if (i != n - 1) {
//                     r += target.getValue(i) + separator;
//                 } else {
//                     r += target.getValue(i);
//                 }
//             }
//             return r;
//         },
//         /**
//          * es5
//          * Reverses the elements in an array in place.
//          * This method mutates the array and returns a reference to the same array.
//          */
//         "reverse": function (): any[] {
//             let n = target.count;
//             let m = Math.floor(n / 2);
//             for (let i = 0; i < m; ++i) {
//                 let i2 = n - i - 1;
//                 let t1 = target.getValue(i2);
//                 let t2 = target.getValue(i);
//                 target.setValue(i, t1);
//                 target.setValue(i2, t2);
//             }
//             return this;
//         },
//         /**
//          * es5
//          * Removes the first element from an array and returns it.
//          * If the array is empty, undefined is returned and the array is not modified.
//          */
//         "shift": function (): any | undefined {
//             if (target.count > 0) {
//                 let f = target.getValue(0);
//                 target.removeAt(0);
//                 return f;
//             }
//         },
//         /**
//          * es5
//          * Returns a copy of a section of an array.
//          * For both start and end, a negative index can be used to indicate an offset from the end of the array.
//          * For example, -2 refers to the second to last element of the array.
//          * @param start The beginning index of the specified portion of the array.
//          * If start is undefined, then the slice begins at index 0.
//          * @param end The end index of the specified portion of the array. This is exclusive of the element at the index 'end'.
//          * If end is undefined, then the slice extends to the end of the array.
//          */
//         "slice": function (start?: number, end?: number): any[] {
//             let n = target.count;
//             let r = [];
//             if (typeof start === "undefined") {
//                 start = 0;
//             } else if (start < 0) {
//                 start = n + start;
//             }
//             if (typeof end === "undefined") {
//                 end = 0;
//             } else if (end < 0) {
//                 end = n + end;
//             }
//             if (start < end) {
//                 for (let i = start; i < end; ++i) {
//                     r.push(target.getValue(i));
//                 }
//             }
//             return r;
//         },
//         /**
//          * es5
//          * Sorts an array in place.
//          * This method mutates the array and returns a reference to the same array.
//          * @param compareFn Function used to determine the order of the elements. It is expected to return
//          * a negative value if the first argument is less than the second argument, zero if they're equal, and a positive
//          * value otherwise. If omitted, the elements are sorted in ascending, ASCII character order.
//          * ```ts
//          * [11,2,22,1].sort((a, b) => a - b)
//          * ```
//          */
//         "sort": function (compareFn?: (a: any, b: any) => number): any {
//             let n = target.count;
//             if (n > 1) {
//                 let r = new Array(...this).sort(compareFn);
//                 for (let i = 0; i < n; ++i) {
//                     target.setValue(i, r[i]);
//                 }
//             }
//             return this;
//         },
//         /**
//          * es5
//          * Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
//          * @param start The zero-based location in the array from which to start removing elements.
//          * @param deleteCount The number of elements to remove.
//          * @param items Elements to insert into the array in place of the deleted elements.
//          * @returns An array containing the elements that were deleted.
//          */
//         "splice": function (start: number, deleteCount: number, ...items: any[]): any[] {
//             let len = target.count;
//             let res = [];
//             if (start < len) {
//                 let m = len - start;
//                 if (typeof deleteCount === "number" && m > deleteCount) {
//                     m = deleteCount;
//                 }
//                 for (let i = 0; i < m; ++i) {
//                     res.push(target.getValue(start));
//                     target.removeAt(start);
//                 }
//                 for (let i = 0, c = items.length; i < c; ++i) {
//                     target.insertValue(start, items[c - i - 1]);
//                 }
//             } else {
//                 for (let i = 0, c = items.length; i < c; ++i) {
//                     target.insertValue(len, items[c - i - 1]);
//                 }
//             }
//             return res;
//         },
//         /**
//          * es5
//          * Inserts new elements at the start of an array, and returns the new length of the array.
//          * @param items Elements to insert at the start of the array.
//          */
//         "unshift": function (...items: any[]): number {
//             for (let i = 0, n = items.length; i < n; ++i) {
//                 target.insertValue(0, items[n - i - 1]);
//             }
//             return target.count;
//         },
//         /**
//          * es5
//          * Returns the index of the first occurrence of a value in an array, or -1 if it is not present.
//          * @param searchElement The value to locate in the array.
//          * @param fromIndex The array index at which to begin the search. If fromIndex is omitted, the search starts at index 0.
//          */
//         "indexOf": function (searchElement: any, fromIndex?: number): number {
//             if (typeof fromIndex !== "number") {
//                 fromIndex = 0;
//             } else {
//                 fromIndex = fromIndex <= 0 ? 0 : Math.floor(fromIndex);
//             }
//             for (let i = fromIndex, n = target.count; i < n; ++i) {
//                 if (target.getValue(i) == searchElement) {
//                     return i;
//                 }
//             }
//             return -1;
//         },
//         /**
//          * es5
//          * Returns the index of the last occurrence of a specified value in an array, or -1 if it is not present.
//          * @param searchElement The value to locate in the array.
//          * @param fromIndex The array index at which to begin searching backward. If fromIndex is omitted, the search starts at the last index in the array.
//          */
//         "lastIndexOf": function (searchElement: any, fromIndex?: number): number {
//             if (typeof fromIndex !== "number") {
//                 fromIndex = target.count - 1;
//             } else {
//                 if (fromIndex < 0) {
//                     fromIndex = target.count + fromIndex;
//                 }
//             }
//             for (let i = fromIndex; i >= 0; --i) {
//                 if (target.getValue(i) == searchElement) {
//                     return i;
//                 }
//             }
//             return -1;
//         },
//         /**
//          * es5
//          * Determines whether all the members of an array satisfy the specified test.
//          * @param predicate A function that accepts up to three arguments. The every method calls
//          * the predicate function for each element in the array until the predicate returns a value
//          * which is coercible to the Boolean value false, or until the end of the array.
//          * @param thisArg An object to which the this keyword can refer in the predicate function.
//          * If thisArg is omitted, undefined is used as the this value.
//          */
//         "every": function (predicate: (value: any, index: number, array: any[]) => unknown, thisArg?: any): boolean {
//             for (let i = 0, n = target.count; i < n; ++i) {
//                 let item = target.getValue(i);
//                 if (!Boolean(predicate.call(thisArg, item, i, this))) {
//                     return false;
//                 }
//             }
//             return true;
//         },
//         /**
//          * es5
//          * Determines whether the specified callback function returns true for any element of an array.
//          * @param predicate A function that accepts up to three arguments. The some method calls
//          * the predicate function for each element in the array until the predicate returns a value
//          * which is coercible to the Boolean value true, or until the end of the array.
//          * @param thisArg An object to which the this keyword can refer in the predicate function.
//          * If thisArg is omitted, undefined is used as the this value.
//          */
//         "some": function (predicate: (value: any, index: number, array: any[]) => unknown, thisArg?: any): boolean {
//             for (let i = 0, n = target.count; i < n; ++i) {
//                 let item = target.getValue(i);
//                 if (Boolean(predicate.call(thisArg, item, i, this))) {
//                     return true;
//                 }
//             }
//             return false;
//         },
//         /**
//          * es5
//          * Performs the specified action for each element in an array.
//          * @param callbackfn  A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array.
//          * @param thisArg  An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.
//          */
//         "forEach": function (callbackfn: (value: any, index: number, array: any[]) => void, thisArg?: any): void {
//             if (typeof thisArg !== "undefined") {
//                 for (let i = 0, n = target.count; i < n; i++) {
//                     callbackfn.call(thisArg, target.getValue(i), i, this);
//                 }
//             } else {
//                 for (let i = 0, n = target.count; i < n; i++) {
//                     callbackfn(target.getValue(i), i, this);
//                 }
//             }
//         },
//         /**
//          * es5
//          * Calls a defined callback function on each element of an array, and returns an array that contains the results.
//          * @param callbackfn A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
//          * @param thisArg An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.
//          */
//         map(callbackfn: (value: any, index: number, array: any[]) => any, thisArg?: any): any[] {
//             let n = target.count;
//             let r = new Array(n);
//             for (let i = 0; i < n; i++) {
//                 let e = callbackfn.call(thisArg, target.getValue(i), i, this);
//                 r.push(e);
//             }
//             return r;
//         },
//         /**
//          * es5
//          * Returns the elements of an array that meet the condition specified in a callback function.
//          * @param predicate A function that accepts up to three arguments. The filter method calls the predicate function one time for each element in the array.
//          * @param thisArg An object to which the this keyword can refer in the predicate function. If thisArg is omitted, undefined is used as the this value.
//          */
//         "filter": function (predicate: (value: any, index: number, array: any[]) => unknown, thisArg?: any): any[] {
//             let res = [];
//             for (let i = 0, n = target.count; i < n; ++i) {
//                 let item = target.getValue(i);
//                 if (Boolean(predicate.call(thisArg, item, i, this))) {
//                     res.push(item);
//                 }
//             }
//             return res;
//         },
//         /**
//          * es5
//          * Calls the specified callback function for all the elements in an array. The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
//          * @param callbackfn A function that accepts up to four arguments. The reduce method calls the callbackfn function one time for each element in the array.
//          * @param initialValue If initialValue is specified, it is used as the initial value to start the accumulation. The first call to the callbackfn function provides this value as an argument instead of an array value.
//          */
//         "reduce": function (callbackfn: (previousValue: any, currentValue: any, currentIndex: number, array: any[]) => any, initialValue: any): any {
//             let n = target.count;
//             if (n > 0) {
//                 let previousValue, currentValue;
//                 if (typeof initialValue !== "undefined") {
//                     previousValue = initialValue;
//                     for (let i = 0; i < n; ++i) {
//                         currentValue = target.getValue(i);
//                         previousValue = callbackfn(previousValue, currentValue, i, this);
//                     }
//                     return previousValue;
//                 } else {
//                     previousValue = target.getValue(0);
//                     for (let i = 1; i < n; ++i) {
//                         currentValue = target.getValue(i);
//                         previousValue = callbackfn(previousValue, currentValue, i, this);
//                     }
//                     return previousValue;
//                 }
//             }
//             return initialValue;
//         },
//         /**
//          * es5
//          * Calls the specified callback function for all the elements in an array, in descending order. The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
//          * @param callbackfn A function that accepts up to four arguments. The reduceRight method calls the callbackfn function one time for each element in the array.
//          * @param initialValue If initialValue is specified, it is used as the initial value to start the accumulation. The first call to the callbackfn function provides this value as an argument instead of an array value.
//          */
//         "reduceRight": function (callbackfn: (previousValue: any, currentValue: any, currentIndex: number, array: any[]) => any, initialValue: any): any {
//             let n = target.count;
//             if (n > 0) {
//                 let previousValue, currentValue;
//                 if (typeof initialValue !== "undefined") {
//                     previousValue = initialValue;
//                     for (let i = n - 1; i >= 0; --i) {
//                         currentValue = target.getValue(i);
//                         previousValue = callbackfn(previousValue, currentValue, i, this);
//                     }
//                     return previousValue;
//                 } else {
//                     previousValue = target.getValue(n - 1);
//                     for (let i = n - 2; i >= 0; --i) {
//                         currentValue = target.getValue(i);
//                         previousValue = callbackfn(previousValue, currentValue, i, this);
//                     }
//                     return previousValue;
//                 }
//             }
//             return initialValue;
//         },
//     }

//     let _getter = {
//         [underlyingValue]: function () {
//             return target;
//         },
//         "length": function () {
//             return target.count;
//         },
//     }

//     let _setter = {
//         "length": function (value: number) {
//             throw new Error("unsupported");
//         },
//     }

//     return <any>new Proxy(target, {
//         get(target: UnderlyingNativeArray, p: string | symbol, receiver: any): any {
//             if (typeof p === "string" && p.length > 0) {
//                 let c = p.charCodeAt(0);
//                 if (c >= 48 && c <= 57) {
//                     let index = Number.parseInt(p);
//                     return target.getValue(index);
//                 }
//             }

//             let mf = Object.getOwnPropertyDescriptor(_members, p) && _members[p];
//             if (typeof mf !== "undefined") {
//                 return mf;
//             }

//             let mp = Object.getOwnPropertyDescriptor(_getter, p) && _getter[p];
//             if (typeof mp !== "undefined") {
//                 return mp();
//             }
//             console.warn("unknown property", p);
//         },

//         set(target: UnderlyingNativeArray, p: string | symbol, value: any, receiver: any): boolean {
//             if (typeof p === "string" && p.length > 0) {
//                 let c = p.charCodeAt(0);
//                 if (c >= 48 && c <= 57) {
//                     let index = Number.parseInt(p);
//                     target.setValue(index, value);
//                     return true;
//                 }
//             }

//             let mp = Object.getOwnPropertyDescriptor(_setter, p) && _setter[p];
//             if (typeof mp !== "undefined") {
//                 mp(value);
//                 return true;
//             }
//             return false;
//         },

//         // getPrototypeOf(target: MyArray): object {
//         //     return Array.prototype;
//         // },

//         ownKeys(target: UnderlyingNativeArray): ArrayLike<string | symbol> {
//             return new Proxy({}, {
//                 get(_1: any, p: string | symbol): any {
//                     if (typeof p === "string") {
//                         if (p === "length") {
//                             return target.count;
//                         }
//                     }
//                     return p;
//                 },
//             });
//         },

//         getOwnPropertyDescriptor(target: UnderlyingNativeArray, p: string | symbol): PropertyDescriptor {
//             return { enumerable: true, configurable: true, value: this[p] };
//         },
//     });
// }
