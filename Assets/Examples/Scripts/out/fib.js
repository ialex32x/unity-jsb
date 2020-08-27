"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.fib = void 0;
/* fib module */
function fib(n) {
    if (n <= 0)
        return 0;
    else if (n == 1)
        return 1;
    else
        return fib(n - 1) + fib(n - 2);
}
exports.fib = fib;
//# sourceMappingURL=fib.js.map