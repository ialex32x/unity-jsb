
export function Inspector(path: string, className: string) {
    return function (target: any) {
        // 暂时简单实现
        target.prototype.__editor__ = require(path)[className];
        return target;
    }
}
