"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Inspector = void 0;
function Inspector(path, className) {
    return function (target) {
        // 暂时简单实现
        target.prototype.__editor__ = require(path)[className];
        return target;
    };
}
exports.Inspector = Inspector;
//# sourceMappingURL=inspector.js.map