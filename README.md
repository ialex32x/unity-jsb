# unity-jsb

# compile for windows on linux
sudo apt-get update
sudo apt-get install mingw-w64

./configure --host=i686-w64-mingw32
x86_64-w64-mingw32

# Examples

```js
// await/async with unity yield-able objects
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

```js
// import module support 
import { fib } from "./fib.js";

// commonjs module support
require("./test");

// commonjs modules cache access
Object.keys(require.cache).forEach(key => console.log(key));
```


# TODO
* [X] console.* 
* [X] commonjs module 
* [X] timer
* [X] unity YieldInstruction => promise
* [ ] websocket
* [ ] sourcemap helper
* [ ] event dispatcher
* [ ] ref 传参时, 从 val.target 进行取值 (因为会需要回写target, 保持一致性)
* [ ] Values_push_class.cs ```public static JSValue js_push_classvalue(JSContext ctx, IO.ByteBuffer o)```
* [ ] Vector3 operator overloading
* [ ] mobile platform build
 