
/**
 * 打印日志
 */
declare function print(...args: any[]): void;

/**
 * 向 master runtime 发送消息 (目前 data 通过 JS_WriteObject/ReadObject 进行传输)
 * !!! 注意: 目前还没有提供跨越 runtime 的托管对象传递, 请不要传递.net托管对象. 
 */
declare function postMessage(data: any): void;

/**
 * [experimental]
 */
declare function jsx(type: string, attributes: any, content?: any): any;

/**
 * this module is only available with `editorScripting`
 */
declare module "jsb.editor" {
    import { Prefs } from "QuickJS.Binding";
    import { TSConfig } from "QuickJS.Utils";

    class EditorRuntime {
        /**
         * the prefs loaded by EditorRuntime 
         */
        static readonly prefs: Prefs;

        /**
         * the tsconfig.json parsed by EditorRuntime
         */
        static readonly tsconfig: TSConfig;
    }
}

/**
 * jsb 模块为内部提供的工具支持
 */
declare module "jsb" {
    import { Delegate as SystemDelegate, Array as SystemArray } from "System";

    type byte = number;
    type Nullable<T> = T;

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

    interface RuntimeInfo {
        /**
         * 运行时唯一ID
         */
        id: number;

        /**
         * 是否是工作者线程
         */
        isWorker: boolean;

        /**
         * 是否是编辑器运行时
         */
        isEditor: boolean;
    }

    /**
     * duck type
     */
    interface Task<T> { }

    /**
     * 标记一个类型仅在特定编译选项下存在 (该修饰器并不存在实际定义, 仅用于标记, 不要在代码中使用)
     */
    function RequiredDefines(...targets: string[]);

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
     * [NotImplemented][未实现]
     */
    class Runtime {
        /**
         * [NotImplemented][未实现] 获取所有 ScriptRuntime 信息
         */
        static get runtimes(): RuntimeInfo[];

        /**
         * [NotImplemented][未实现] 从其他 ScriptRuntime 接收消息
         */
        static get onmessage(): (id: number, data: any) => void;
        static set onmessage(cb: (id: number, data: any) => void);

        /**
         * [NotImplemented][未实现] 向指定 ScriptRuntime 发送消息
         */
        static postMessage(id: number, data: any): void;
    }

    class ModuleManager {
        static BeginReload();

        /**
         * 将此模块标记为等待重载
         */
        static MarkReload(moduleId: string);

        static EndReload();
    }

    /**
     * 执行指定脚本 (慎用, 与 webpack 等工具的结合性可能不太好)
     */
    function DoFile(filename: string): void;

    /**
     * 强行执行一次完整垃圾回收
     */
    function GC(): void

    /**
     * 标记对象是否由JS管理销毁 (自动调用 Dispose)
     * @param o CS对象实例
     * @param disposable 是否托管
     * @returns 是否设置成功
     */
    function SetDisposable(o: any, disposable: boolean): boolean;

    /**
     * setInterval/setTimeout 所用定时管理器的当前时间
     */
    function Now(): number

    /**
     * 是否运行在 StaticBinding 模式下
     */
    function IsStaticBinding(): boolean;

    let isOperatorOverloadingSupported: boolean;

    /**
     * indicates what backend engine is used
     */
    let engine: string;

    /**
     * version of jsb dll
     */
    let version: number;

    /**
     * 即 Thread.Sleep()
     */
    function Sleep(millisecondsTimeout?: number): void

    /**
     * 将字符串添加到缓存中, 可以避免在使用此字符串频繁进行 C#/JS 交互时产生字符串构造.
     */
    function AddCacheString(str: string): string;

    /**
     * 移除字符串缓存
     */
    function RemoveCacheString(str: string): boolean;

    /**
     * 将指定路径添加到 duktape 加载脚本的搜索目录列表
     */
    function AddSearchPath(path: string): void

    /**
     * [UNDOCUMENTED]
     */
    function AddModule(module_id: string, e: any): void;

    // @ts-ignore
    // function Yield(instruction: UnityEngine.YieldInstruction): Promise<UnityEngine.YieldInstruction>;
    // function Yield(enumerator: SystemCollections.IEnumerator): Promise<SystemObject>;
    function Yield<T>(task: Task<T>): Promise<T>;

    /**
     * 将 C# 委托强制转换为 JS Function
     * @summary 如果传入的已经是 Function, 将直接返回
     * @summary 如果传入的是一个 C# 委托对象, 将通过 dynamic method wrapper 产生一个 JS Function 
     * @summary 谨慎: 无法再从 function 还原此委托, 两者不会建立关联 (会构成强引用循环) 
     * @summary 谨慎: NewDynamicDelegate 会产生一个与 Runtime 相同生命周期的对象, 该对象将持有 Delegate 对象引用 
     * @param makeDynamic 是否创建委托的JS函数包装 (NewDynamicDelegate) [默认 false]
     */
    function ToFunction(o: SystemDelegate | Function, makeDynamic?: boolean): Function;

    /**
     * [待定]
     */
    function ToDelegate(o: SystemDelegate | Function, typeName: string): SystemDelegate;

    // function ToJSArray(o: any): Array;
    /**
     * 将 C# 数组转换为 JS 数组
     */
    function ToArray<T>(o: SystemArray<T>): Array<T>;

    /**
     * 将 C# 数组转换为 JS ArrayBuffer
     */
    function ToArrayBuffer(o: SystemArray<byte> | number[]): ArrayBuffer;

    /**
     * 将 JS ArrayBuffer 转换为 C# Array
     */
    function ToBytes(o: ArrayBuffer | Uint8Array): SystemArray<byte>;

    /**
     * 动态载入指定类型
     * @param type 完整类型名
     * @param privateAccess 是否允许访问私有成员
     */
    function Import(type: string, privateAccess?: boolean): FunctionConstructor;
}
