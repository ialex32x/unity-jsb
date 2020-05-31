

declare function print(...args: any[]): void;

/**
 * 执行指定脚本 （类似eval）
 * @param source 脚本源码
 * @param filename 为此脚本指定命名 
 */
declare function dostring(source: string, filename?: string): void

/**
 * 执行指定脚本
 */
declare function dofile(filename: string): void

/**
 * 将指定路径添加到 duktape 加载脚本的搜索目录列表
 */
declare function addSearchPath(path: string): void

/**
 * 是否开启 print 函数的 stacktrace 输出 (默认关闭)
 */
declare function enableStacktrace(enabled: boolean): void

declare namespace jsb {
    const DUK_VERSION: string
    
    const COMPLETE: string
    const ERROR: string

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
    interface Ref<T> {
        target: T
    }

    /**
     * 封装 C# out 传参约定
     */
    interface Out<T> {
        target: T
    }

    /**
     * 监听者
     */
    class Handler {
        constructor(caller: any, fn: Function, once: boolean)
        /**
         * 判断是否与指定的 caller, fn 组合等价, 不指定 fn 时, 只要 caller 相等即为等价
         */
        equals(caller: any, fn?: Function): boolean
        invoke(...args: any[]): any
    }

    /**
     * 监听者列表
     * 注意: 当前 off 只是对 handlers 数组进行删除标记, 下次 on 时将复用, 所以并不能严格遵守 on 的顺序
     */
    class Dispatcher {
        readonly handlers: Array<Handler>
        
        constructor()

        /**
         * 添加监听
         * @param caller 回调函数执行时绑定的 this
         * @param fn 回调函数
         * @param once 是否单次出发, 默认 false
         */
        on(caller: any, fn: Function, once?: boolean): Dispatcher

        /**
         * 移除监听
         * @param caller 移除指定 caller 对应的回调
         * @param fn 移除指定回调, 不指定 fn 时, 移除所有 caller 注册的回调
         */
        off(caller: any, fn?: Function): void

        /**
         * 触发事件
         */
        dispatch(...args: any[]): any

        clear(): void
    }

    class EventDispatcher {
        readonly events: { [type: string]: Dispatcher }

        constructor()
        /**
         * 添加监听
         * @param caller 回调函数执行时绑定的 this
         * @param fn 回调函数
         * @param once 是否单次出发, 默认 false
         */
        on(type: string, caller: any, fn: Function, once?: boolean): Dispatcher

        /**
         * 移除监听
         * @param caller 移除指定 caller 对应的回调
         * @param fn 移除指定回调, 不指定 fn 时, 移除所有 caller 注册的回调
         */
        off(type: string, caller: any, fn?: Function): void

        /**
         * 触发事件
         */
        dispatch(type: string, ...args: any[]): any

        clear(type: string): void
    }

    class Enum {
        static GetName(type: any, val: number): string
    }

    class Array {
        static Create(type: any, size: number): any
    }

    class CSharp {
        /**
         * [dangerous] List<T>
         */
        CreateList(type: any): any 
        GetType(name: string): any
        IsNull(v: any): boolean
    }

    // class Delegate {
    //     static on<R>(caller: any, fn: () => R): Delegate
    //     static on<R, T0>(caller: any, fn: (arg0: T0) => R): Delegate
    //     static on<R, T0, T1>(caller: any, fn: (arg0: T0, arg1: T1) => R): Delegate
    // }

    class WebSocket extends EventDispatcher {
        readonly connected: boolean
        constructor(protocols?: Array<string>)
        
        connect(url: string, ssl_verify?: boolean)
        close()
        poll()
        send(data: any): boolean
    }

    enum SocketType {
        TCP = 0,
        UDP = 1, 
    }

    enum SocketFamily {
        IPV4 = 0,
        IPV6 = 1, 
    }

    /**
     * raw socket [experimental]
     */
    class Socket {
        constructor(type: SocketType, family: SocketFamily);
        setnonblocking(): void;
        connect(host: string, port: number);
        // connect_addr(addr: Buffer, len: number);
        close(): void;
        send(text: string): number;
        send(buf: Buffer, index: number, size: number): number;
        recv(buf: Buffer, index: number, size: number): number;
    }

    /**
     * kcp [experimental]
     */
    class Kcp {
        constructor(conv: number, family: SocketFamily, bufferSize: number);
        connect(host: string, port: number);
        // connect_addr(addr: Buffer, len: number);
        wndsize(sndwnd: number, rcvwnd: number);
        nodelay(nodelay: number, interval: number, resend: number, nc: number);
        update(current: number);
        /**
         * 返回值 <0 时异常
         */
        send(text: string): number; 
        /**
         * 返回值 <0 时异常
         */
        send(buf: Buffer, size?: number): number;
        recv(buf: Buffer, index: number, size: number): number;
        close(): void;
    }
    
    /*
    class Socket extends Dispatcher {
        readonly connected: boolean

        constructor()
        connect(host: string, port: number): void
        close(): void
        send(data: any): void
    }

    class HttpRequest extends Dispatcher {
        send(url: string, data?: any, method?: string, type?: string, headers?: any[]): void
    }
    */
}

/**
 * polyfills for es5
 */
declare interface Object {
    /**
     * Sets the prototype of a specified object o to  object proto or null. Returns the object o.
     * @param o The object to change its prototype.
     * @param proto The value of the new prototype or null.
     */
    setPrototypeOf(o: any, proto: object | null): any
}
