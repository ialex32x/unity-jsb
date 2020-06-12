# unity-jsb

# compile for windows on linux
sudo apt-get update
sudo apt-get install mingw-w64

./configure --host=i686-w64-mingw32
x86_64-w64-mingw32

# Examples

```ts
// 支持 js class 直接继承 MonoBehaviour 
// 所有响应函数支持异步函数
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

```ts
// 支持 await/async
// 支持异步函数与Unity等待直接结合使用
async function testAsyncFunc () {
    console.log("you can await any Unity YieldInstructions");
    await jsb.Yield(new UnityEngine.WaitForSeconds(1.2));
    await jsb.Yield(null);

    console.log("setTimeout support")
    await new Promise(resolve => {
        setTimeout(() => resolve(), 1000);    
    });
}

testAsyncFunc();
```

```ts
// 支持重载运算符 (部分)
{
    let vec1 = new UnityEngine.Vector3(1, 2, 3);
    let vec2 = new UnityEngine.Vector3(9, 8, 7);
    let vec3 = vec1 + vec2;
    console.log(vec3.ToString());
}
{
    let vec1 = new UnityEngine.Vector2(1, 2);
    let vec2 = new UnityEngine.Vector2(9, 8);
    let vec3 = vec1 + vec2;
    console.log(vec3.ToString());
}
```

```ts
// 支持 ES6 模块 (import)
import { fib } from "./fib.js";

// 支持 commonjs 模块 (基础支持) (node.js 'require')
require("./test");

// commonjs modules cache access
Object.keys(require.cache).forEach(key => console.log(key));
```


# TODO
* [X] console.* 
* [X] commonjs module 
* [X] timer
* [X] unity YieldInstruction => promise
* [X] JSMemoryUsage window
* [ ] compile into bytecode
* [ ] websocket
* [ ] sourcemap helper
* [ ] event dispatcher
* [ ] ref 传参时, 从 val.target 进行取值 (因为会需要回写target, 保持一致性)
* [ ] Values_push_class.cs ```public static JSValue js_push_classvalue(JSContext ctx, IO.ByteBuffer o)```
* [ ] Vector3 operator overloading
* [ ] mobile platform build
 