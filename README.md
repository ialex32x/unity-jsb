# unity-jsb

使用 [QuickJS](https://bellard.org/quickjs/) 为 Unity3D 项目提供 Javascript 运行时支持.  <br/>
通过生成静态绑定代码的方式提供性能良好的 C#/JS 互操作支持. 支持移动平台.

> QuickJS is a small and embeddable Javascript engine. It supports the ES2020 specification including modules, asynchronous generators, proxies and BigInt. 

# 特性支持
* 支持在JS异步函数中等待 Unity YieldInstruction 对象
* 支持在JS异步函数中等待 System.Threading.Tasks.Task 对象 (limited support)
* 向 JS 导入 C# 运算符重载 +, -, *, /, ==, -(负)
* 支持 Websocket (limited support)
* [初步] 未导出的类型通过反射方式进行 C#/JS 交互
* [初步] 运行时替换 C# 代码 (hotfix, limited support)
* [未完成] 支持 JS 字节码 (QuickJS)
* [未完成] 运行时模块热替换 (debug only)

# 特性示例
> 推荐使用 typescript 编写脚本, unity-jsb 对导出的 C# 类型自动生成了对应的 d.ts 声明, 以提供强类型辅助. 示例代码均使用 typescript. <br/>
> 也可以根据喜好选择 coffeescript/clojurescript 等任何可以编译成 javascript 的语言. 最终运行的都是 javascript.

## MonoBehaviour in Javascript
> 支持 JS class 直接继承 MonoBehaviour <br/>
> 所有响应函数支持 JS 异步函数 <br/>
```ts
class MyClass extends UnityEngine.MonoBehaviour {
    protected _tick = 0;

    Awake() {
        console.log("MyClass.Awake", this._tick++);
    }

    async OnEnable() {
        console.log("MyClass.OnEnable", this._tick++);
        await jsb.Yield(new UnityEngine.WaitForSeconds(1));
        console.log("MyClass.OnEnable (delayed)", this._tick++);
    }

    OnDisable() {
        console.log("MyClass.OnDisable", this._tick++);
    }

    OnDestroy() {
        console.log("MyClass.OnDestroy", this._tick++);
    }

    async test() {
        console.log("MyClass.test (will be destroied after 5 secs.", this.transform);
        await jsb.Yield(new UnityEngine.WaitForSeconds(5));
        UnityEngine.Object.Destroy(this.gameObject);
    }
}

class MySubClass extends MyClass {
    Awake() {
        super.Awake();
        console.log("MySubClass.Awake", this._tick++);
    }

    play() {
        console.log("MySubClass.play");
    }
}

let gameObject = new UnityEngine.GameObject();
let comp = gameObject.AddComponent(MySubClass);

comp.play();

let comp_bySuperClass = gameObject.GetComponent(MyClass);
comp_bySuperClass.test();
```

## 异步调用
* 支持 await/async 
* 支持JS异步函数直接等待 Unity 协程的等待对象直接结合使用 
* 支持JS异步函数直接等待 C# Task (但JS环境本身并不支持多线程) 
```ts
async function testAsyncFunc () {
    console.log("you can await any Unity YieldInstructions");
    await jsb.Yield(new UnityEngine.WaitForSeconds(1.2));
    await jsb.Yield(null);

    console.log("setTimeout support")
    await new Promise(resolve => {
        setTimeout(() => resolve(), 1000);    
    });

    // System.Threading.Tasks.Task<System.Net.IPHostEntry>
    let result = <System.Net.IPHostEntry> await jsb.Yield(jsb.AsyncTaskTest.GetHostEntryAsync("www.baidu.com"));
    console.log("host entry:", result.HostName);
}

testAsyncFunc();
```

## 重载运算符

```ts
{
    let vec1 = new UnityEngine.Vector3(1, 2, 3);
    let vec2 = new UnityEngine.Vector3(9, 8, 7);
    // 此特性目前不是js标准, 带语法提示的编辑器通常会提示错误, 但并不影响执行
    // 不希望看到错误提示的可以添加 hint, 比如 vscode 下添加如下标记即可
    // @ts-ignore
    let vec3 = vec1 + vec2; 
    let vec4 = vec1 + vec2;
    console.log(vec3);
    console.log(vec3 / 3);
    console.log(vec3 == vec4);
}
{
    let vec1 = new UnityEngine.Vector2(1, 2);
    let vec2 = new UnityEngine.Vector2(9, 8);
    let vec3 = vec1 + vec2;
    console.log(vec3);
}
```

## 支持模块

```ts
// 支持 ES6 模块 (import)
import { fib } from "./fib.js";

// 支持 commonjs 模块 (基础支持) (node.js 'require')
require("./test");

// commonjs modules cache access
Object.keys(require.cache).forEach(key => console.log(key));
```

## 支持 WebSocket

```ts
let ws = new WebSocket("ws://127.0.0.1:8080/websocket", "default");

console.log("websocket connecting:", ws.url);

ws.onopen = function () {
    console.log("[ws.onopen]", ws.readyState);
    let count = 0;
    setInterval(function () {
        ws.send("websocket message test" + count++);
    }, 1000);
};
ws.onclose = function () {
    console.log("[ws.onclose]", ws.readyState);
};
ws.onerror = function (err) {
    console.log("[ws.onerror]", err);
};
ws.onmessage = function (msg) {
    console.log("[ws.recv]", msg);
};
```

## 支持 Hotfix (初步功能)
```ts

jsb.hotfix.replace_single("HotfixTest", "Foo", function (x: number) {
    // 注入后, 可以访问类的私有成员
    // 如果注入的方法是实例方法, this 将被绑定为 C# 对象实例 
    // 如果注入的方法是静态方法, this 将被绑定为 C# 类型 (对应的 JS Constructor)
    console.log("replace C# method body by js function", this.value); 
    return x;
});

jsb.hotfix.before_single("HotfixTest", "AnotherCall", function () {
    print("在 C# 代码之前插入执行");
});

//NOTE: 如果 HotfixTest 已经是静态绑定过的类型, 注入会导致此类型被替换为反射方式进行 C#/JS 交互, 并且可以访问私有成员
// 目前只能注入简单的成员方法
// 需要先执行 /JS Bridge/Generate Binding 生成对应委托的静态绑定, 再执行 /JS Bridge/Hotfix 修改 dll 后才能在 JS 中进行注入

```

### 对导出的 Unity API 附加了自带文档说明
![unity_ts_docs](jsb_build/res/unity_ts_docs.png)

### 利用强类型提供代码提示
![ts_code_complete](jsb_build/res/ts_code_complete.png)

# compile for windows on linux

```sh
sudo apt-get update
sudo apt-get install mingw-w64

# i686-w64-mingw32
# x86_64-w64-mingw32
```

# 多线程
> 暂不支持多线程访问

# Debugger
> 暂不支持

# 性能
> 待测试

# 状态
> 完成度 ~70%

# 对 QuickJS 的修改
```c
// quickjs.c

// 1: 
// 在不存在log2的情况下提供替代的 log2 
#if defined(JSB_DEF_LOG2)
static double log2(double v) { return log(v) / log(2.0); }
#endif

// 2:
// make JS_GetActiveFunction extern
JSValueConst JS_GetActiveFunction(JSContext *ctx) { }

```

# Referenced libraries

* [QuickJS](https://bellard.org/quickjs/)
* [ECMAScript](https://github.com/Geequlim/ECMAScript.git) A Javascript (QuickJS) Binding for Godot 
* [xLua](https://github.com/Tencent/xLua)
* [libwebsockets](https://github.com/warmcat/libwebsockets)
* [mbedtls](https://github.com/ARMmbed/mbedtls)
* [zlib](https://zlib.net/)

# TODO
* [X] console.* 基本的兼容性 
* [X] commonjs 模块 基本的兼容性 
* [X] 支持 timer (setTimeout/setInterval)
* [X] setImmediate
* [X] sourcemap 转换 JS 调用栈
* [X] 针对嵌套类型的 Binding 过程调整
* [X] 静态 Bind 过程
* [X] compile into JS bytecode (QuickJS)
* [X] Values_push_class.cs ```public static JSValue js_push_classvalue(JSContext ctx, IO.ByteBuffer o)```
* [ ] jsb.Yield C# side wrapper as JSPromise.Resolve()
* [ ] event dispatcher
* [X] 完善 Ref/Out 参数的处理 (改为多返回值模拟方式)
* [X] mobile platform build: android
* [ ] mobile platform build: ios
* [ ] 静态绑定和反射绑定对重载的处理顺序可能不同
* [ ] 静态绑定和反射绑定对参数类型的判断可能不同
* [X] 静态绑定的类型也可以进行 hotfix
* [X] hotfix replace (注入il, 初步功能, 只处理了有限支持的 Method)
* [X] hotfix 注入构造函数
* [ ] hotfix 注入 Property
* [X] hotfix before
* [ ] hotfix after
* [X] 静态绑定也提供 Private Access (不直接支持, 运行时使用反射绑定替代静态绑定来支持此特性)
* [ ] 可以安全的保留 ScriptValue 对象引用
* [ ] 整理 Examples
* [X] wiki doc 创建

# 待定
* [ ] Worker API
* [ ] WebAssembly build


