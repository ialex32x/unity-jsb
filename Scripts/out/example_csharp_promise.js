"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const global_1 = require("global");
const jsb_1 = require("jsb");
const UnityEngine_1 = require("UnityEngine");
async function test_custom_promise() {
    print("想象3秒后弹出的是个对话框");
    await jsb_1.Yield(new UnityEngine_1.WaitForSeconds(3));
    let go = new UnityEngine_1.GameObject("custome.promise.test");
    var sb = go.AddComponent(global_1.SampleBehaviour);
    print("想象这里是等待用户点击对话框按钮");
    print("完成 (promise in C#):", await sb.SimpleWait(1));
}
test_custom_promise();
//# sourceMappingURL=example_csharp_promise.js.map