"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Example_1 = require("Example");
const jsb_1 = require("jsb");
if (module == require.main) {
    let actions = new Example_1.DelegateTest();
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
    Example_1.DelegateTest.onStaticEvent("add", v => print("测试静态事件1:", v));
    function staticEventHandler(v) { print("测试静态事件2:", v); }
    Example_1.DelegateTest.onStaticEvent("add", staticEventHandler);
    Example_1.DelegateTest.DipatchStaticEvent(123);
    Example_1.DelegateTest.onStaticEvent("remove", staticEventHandler);
    Example_1.DelegateTest.DipatchStaticEvent(123);
    if (!jsb_1.IsReflectBind()) {
        print("测试: 带 ref/out 的委托");
        actions.complexCall("add", (b, a, v) => {
            a.value += b;
            v.value = 999;
            return 789;
        });
        actions.TestComplexCall();
    }
    else {
        console.warn("reflectbind 模式不支持带 ref,out 参数的委托");
    }
}
//# sourceMappingURL=example_delegate.js.map