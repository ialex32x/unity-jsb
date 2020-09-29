if (module == require.main) {
    let actions = new jsb.DelegateTest();
    print("测试: 无参数的委托");
    console.log("********** add");
    actions.onAction("add", function () {
        console.log("js action call");
    });
    console.log("********** call");
    actions.CallAction();
    actions.onAction("set", null);
    console.log("********** after clear, call again");
    actions.CallAction();
    print("测试: 带参数的委托");
    actions.onActionWithArgs("set", (a, b, c) => {
        console.log(a, b, c);
    });
    actions.CallActionWithArgs("string", 123, 456);
    actions.onFunc("set", v => v * 2);
    console.log(actions.CallFunc(111));
    actions.onFunc("set", undefined);
    print("测试: 事件");
    actions.onEvent("add", v => print("测试事件1:", v));
    function instanceEventHandler(v) { print("测试事件2:", v); }
    actions.onEvent("add", instanceEventHandler);
    actions.DipatchEvent(123);
    actions.onEvent("remove", instanceEventHandler);
    actions.DipatchEvent(123);
    print("测试: 静态事件");
    jsb.DelegateTest.onStaticEvent("add", v => print("测试静态事件1:", v));
    function staticEventHandler(v) { print("测试静态事件2:", v); }
    jsb.DelegateTest.onStaticEvent("add", staticEventHandler);
    jsb.DelegateTest.DipatchStaticEvent(123);
    jsb.DelegateTest.onStaticEvent("remove", staticEventHandler);
    jsb.DelegateTest.DipatchStaticEvent(123);
}
//# sourceMappingURL=example_delegate.js.map