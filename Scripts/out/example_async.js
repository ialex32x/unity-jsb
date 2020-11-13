"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Example_1 = require("Example");
const jsb_1 = require("jsb");
const UnityEngine_1 = require("UnityEngine");
function delay(secs) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            print("[async] resolve");
            resolve(123);
        }, secs * 1000);
    });
}
async function test() {
    print("[async] begin");
    await delay(3);
    print("[async] end");
    let result = await jsb_1.Yield(Example_1.AsyncTaskTest.GetHostEntryAsync("www.baidu.com"));
    console.log("host entry:", result.HostName);
    await jsb_1.Yield(Example_1.AsyncTaskTest.SimpleTest(3000));
    console.log("after AsyncTaskTest.SimpleTest(1000)");
}
async function testUnityYieldInstructions() {
    console.warn("wait for unity YieldInstruction, begin");
    await jsb_1.Yield(new UnityEngine_1.WaitForSeconds(3));
    console.warn("wait for unity YieldInstruction, end;", UnityEngine_1.Time.frameCount);
    await jsb_1.Yield(null);
    console.warn("wait for unity YieldInstruction, next frame;", UnityEngine_1.Time.frameCount);
}
test();
testUnityYieldInstructions();
//# sourceMappingURL=example_async.js.map