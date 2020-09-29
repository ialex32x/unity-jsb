
/**
 * 打印日志
 */
declare function print(...args: any[]): void;

declare function postMessage(data: any): void;

// declare function require(id: string): any;

// /**
//  * 执行指定脚本 （类似eval）
//  * @param source 脚本源码
//  * @param filename 为此脚本指定命名 
//  */
// declare function dostring(source: string, filename?: string): void

// /**
//  * 是否开启 print 函数的 stacktrace 输出 (默认关闭)
//  */
// declare function enableStacktrace(enabled: boolean): void

declare namespace jsb {

    /**
     * 仅用于声明
     */
    type byte = number;

    /**
     * 可空类型返回值 (仅用于声明)
     */
    type Nullable<T> = T;

    /**
     * 执行指定脚本 (慎用, 与 webpack 等工具的结合性可能不太好)
     */
    function DoFile(filename: string): void

    function GC(): void

    function Sleep(millisecondsTimeout?: number): void

    function AddCacheString(str: string): string;

    function RemoveCacheString(str: string): boolean;

    /**
     * 将指定路径添加到 duktape 加载脚本的搜索目录列表
     */
    function AddSearchPath(path: string): void

    /**
     * 标记一个类型仅编辑器环境可用 (该修饰器并不存在实际定义, 仅用于标记, 不要在代码中使用)
     */
    function EditorRuntime(target: any);

    /**
     * 替换C#代码执行 (未完成此功能)
     */
    namespace hotfix {
        /**
         * 替换 C# 方法 (此方法为测试方法, 最终将移除)
         */
        function replace_single(type: string, func_name: string, func: Function): void
        /**
         * 在 C# 方法前插入执行 (此方法为测试方法, 最终将移除)
         */
        function before_single(type: string, func_name: string, func: Function): void

        /**
         * 在 C# 方法执行前插入执行 (未完成此功能)
         */
        function before(type: string, func_name: string, func: Function): void

        /**
         * 在 C# 方法执行后插入执行 (未完成此功能)
         */
        function after(type: string, func_name: string, func: Function): void
    }

    /**
     * duck type
     */
    interface Task<T> {
    }

    // @ts-ignore
    // function Yield(instruction: UnityEngine.YieldInstruction): Promise<UnityEngine.YieldInstruction>;
    // function Yield(enumerator: System.Collections.IEnumerator): Promise<System.Object>;
    function Yield<T>(task: Task<T>): Promise<T>;

    /**
     * 将 C# 委托强制转换为 JS Function
     * @summary 如果传入的已经是 Function, 将直接返回
     * @summary 如果传入的是一个 C# 委托对象, 将通过 dynamic method wrapper 产生一个 JS Function 
     * @summary 谨慎: 无法再从 function 还原此委托, 两者不会建立关联 (会构成强引用循环) 
     * @summary 谨慎: NewDynamicDelegate 会产生一个与 Runtime 相同生命周期的对象, 该对象将持有 Delegate 对象引用 
     * @param makeDynamic 是否创建委托的JS函数包装 (NewDynamicDelegate) [默认 false]
     */
    function ToFunction(o: System.Delegate | Function, makeDynamic?: boolean): Function;

    /**
     * [待定]
     */
    function ToDelegate(o: System.Delegate | Function, typeName: string): System.Delegate;

    // function ToJSArray(o: any): Array;
    /**
     * 将 C# 数组转换为 JS 数组
     */
    function ToArray<T>(o: System.Array<T>): Array<T>;

    /**
     * 将 C# 数组转换为 JS ArrayBuffer
     */
    function ToArrayBuffer(o: System.Array<jsb.byte>): ArrayBuffer;

    /**
     * 将 JS ArrayBuffer 转换为 C# Array
     */
    function ToBytes(o: ArrayBuffer | Uint8Array): System.Array<jsb.byte>;

    function Import(type: string, privateAccess?: boolean): FunctionConstructor;

    /**
     * 封装 C# event 调用约定
     */
    interface event<T> {
        on(fn: T): void
        off(fn: T): void
    }

    /**
     * 封装 C# ref 传参约定
     */
    interface Ref<T = any> {
        type?: { new(): T } | Function
        value?: T
    }

    /**
     * 封装 C# out 传参约定
     */
    interface Out<T = any> {
        type?: { new(): T } | Function
        value?: T
    }

    // /**
    //  * 监听者
    //  */
    // class Handler {
    //     constructor(caller: any, fn: Function, once: boolean)
    //     /**
    //      * 判断是否与指定的 caller, fn 组合等价, 不指定 fn 时, 只要 caller 相等即为等价
    //      */
    //     equals(caller: any, fn?: Function): boolean
    //     invoke(...args: any[]): any
    // }

    // /**
    //  * 监听者列表
    //  * 注意: 当前 off 只是对 handlers 数组进行删除标记, 下次 on 时将复用, 所以并不能严格遵守 on 的顺序
    //  */
    // class Dispatcher {
    //     readonly handlers: Array<Handler>

    //     constructor()

    //     /**
    //      * 添加监听
    //      * @param caller 回调函数执行时绑定的 this
    //      * @param fn 回调函数
    //      * @param once 是否单次出发, 默认 false
    //      */
    //     on(caller: any, fn: Function, once?: boolean): Dispatcher

    //     /**
    //      * 移除监听
    //      * @param caller 移除指定 caller 对应的回调
    //      * @param fn 移除指定回调, 不指定 fn 时, 移除所有 caller 注册的回调
    //      */
    //     off(caller: any, fn?: Function): void

    //     /**
    //      * 触发事件
    //      */
    //     dispatch(...args: any[]): any

    //     clear(): void
    // }

    // class EventDispatcher {
    //     readonly events: { [type: string]: Dispatcher }

    //     constructor()
    //     /**
    //      * 添加监听
    //      * @param caller 回调函数执行时绑定的 this
    //      * @param fn 回调函数
    //      * @param once 是否单次出发, 默认 false
    //      */
    //     on(type: string, caller: any, fn: Function, once?: boolean): Dispatcher

    //     /**
    //      * 移除监听
    //      * @param caller 移除指定 caller 对应的回调
    //      * @param fn 移除指定回调, 不指定 fn 时, 移除所有 caller 注册的回调
    //      */
    //     off(type: string, caller: any, fn?: Function): void

    //     /**
    //      * 触发事件
    //      */
    //     dispatch(type: string, ...args: any[]): any

    //     clear(type: string): void
    // }
}
