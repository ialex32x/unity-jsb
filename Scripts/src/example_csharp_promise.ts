import { SampleBehaviour } from "global";
import { Yield } from "jsb";
import { WaitForSeconds, GameObject } from "UnityEngine";

async function test_custom_promise1() {
    print("想象3秒后弹出的是个对话框");
    await Yield(new WaitForSeconds(3));
    let go = new GameObject("custome.promise.test1");
    let sb = go.AddComponent(SampleBehaviour);
    print("想象这里是等待用户点击对话框按钮");
    print("完成 (promise in C#):", await sb.SimpleWait(1));
}

async function test_custom_promise2() {
    let go = new GameObject("custome.promise.test2");
    let sb = go.AddComponent(SampleBehaviour);
    await sb.AnotherWait(123);
    console.log("after another wait");
}

test_custom_promise1();
test_custom_promise2();
