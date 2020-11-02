import { SampleBehaviour } from "global";
import { Yield } from "jsb";
import { WaitForSeconds, GameObject } from "UnityEngine";

async function test_custom_promise() {
    print("想象3秒后弹出的是个对话框");
    await Yield(new WaitForSeconds(3));
    let go = new GameObject("custome.promise.test");
    var sb = go.AddComponent(SampleBehaviour);
    print("想象这里是等待用户点击对话框按钮");
    print("完成 (promise in C#):", await sb.SimpleWait(1));
}

test_custom_promise();
