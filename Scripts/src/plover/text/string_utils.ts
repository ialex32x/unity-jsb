import * as jsb from "jsb";

export let DefaultMatcher = /\$\{([^\{^\}]+)\}/g;

let _stringCache = new Set<string>();

export function TEXT(s: string): string {
    if (!_stringCache.has(s)) {
        _stringCache.add(s);
        jsb.AddCacheString(s);
    }
    return s;
}

/**
 * 按一个上下文对象替换字符串中相应的关键字匹配串
 */
export class Contextual {
    private re: RegExp;
    private context: any;

    constructor(re: RegExp = DefaultMatcher) {
        this.re = re;
    }

    /**
     * 绑定一个上下文对象 （通常是一个map）
     */
    bind(context: any) {
        this.context = context;
        return this;
    }

    /**
     * 替换字符串匹配串 (可以传入上下文，否则使用绑定的)
     * context 可以是一个支持嵌套数据的map, 也可以是一个处理函数.
     * 当使用处理函数时, 参数为取值key字符串.
     */
    replace(text: string, context?: any) {
        return text.replace(this.re, substring => {
            let variable = substring.substring(2, substring.length - 1);
            let ctx = context || this.context;
            if (typeof ctx == "function") {
                return ctx(variable);
            } else {
                let elements = variable.split(".");
                for (let i = 0; i < elements.length; i++) {
                    let element = elements[i];
                    ctx = ctx[element];
                }
                return ctx;
            }
        });
    }

    /**
     * let data = {
     *     name: "test",
     *     company: {
     *         name: "unity",
     *     },
     * }
     * let x = Contextual.replace("hello, mm: ${name} ${company.name}", data)
     * console.log(x);
     */
    static replace(text: string, context: any, re: RegExp = DefaultMatcher) {
        return new Contextual(re).replace(text, context);
    }
}

export class StringUtil {
    /**
     * 替换字符串中的字串
     * @param oldString 原字符串
     * @param matchString 匹配字串
     * @param replaceString 替换字串
     * @returns 替换后的字符串
     */
    static replaceAll(oldString: string, matchString: string, replaceString: string) {
        return oldString.replace(new RegExp(matchString, "g"), replaceString);
    }

    static contains(str: string, match: string) {
        return str.search(new RegExp(match, "i")) > 0;
    }

    /**
     * 数字 => 前缀0固定长度字符串
     */
    static prefix(num: number | string, length: number) {
        let n = num.toString();
        let p = length - n.length;
        if (p <= 0) {
            return n;
        }
        return Array(p + 1).join('0') + n;
    }

    /**
     * 转换为固定小数和整数部分长度的字符串
     */
    static prefix2(num: number, length1: number, length2: number) {
        let p = num.toString().split(".");
        if (p.length == 1) {
            return this.prefix(p[0], length1) + "." + this.prefix(0, length2);
        }
        return this.prefix(p[0], length1) + "." + this.prefix(p[1].substring(0, length2), length2);
    }

    /**
     * 简单字符串表示的时长 (mm:ss.mmm)
     */
    static time(deltaTime: number) {
        let nmsec = deltaTime % 999
        let fsec = Math.floor(deltaTime / 1000)
        let nsec = fsec % 60
        let fmin = Math.floor(fsec / 60)
        let text = fmin < 10 ? "0" + fmin : fmin.toString()
        text += nsec < 10 ? ":0" + nsec : ":" + nsec
        text += nmsec < 10 ? ".00" + nmsec : (nmsec < 100 ? ".0" + nmsec : "." + nmsec)
        return text
    }
}