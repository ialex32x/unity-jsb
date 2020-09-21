async function test_custom_promise() {
    print("想象3秒后弹出的是个对话框");
    await jsb.Yield(new UnityEngine.WaitForSeconds(3));
    let go = new UnityEngine.GameObject("custome.promise.test");
    var sb = go.AddComponent(SampleBehaviour);
    print("想象这里是等待用户点击对话框按钮");
    print("完成 (promise in C#):", await sb.SimpleWait(1));
}
test_custom_promise();
//# sourceMappingURL=example_csharp_promise.js.map