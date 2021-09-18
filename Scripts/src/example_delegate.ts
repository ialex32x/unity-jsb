import { DelegateTest } from "Example";
import { IsStaticBinding } from "jsb";

if (module == require.main) {
    let actions = new DelegateTest();

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
    function instanceEventHandler(v: number) { print("测试事件2:", v) }
    actions.onEvent("add", instanceEventHandler);
    actions.DipatchEvent(123);
    actions.onEvent("remove", instanceEventHandler);
    actions.DipatchEvent(123);

    print("测试: 静态事件");
    DelegateTest.onStaticEvent("add", v => print("测试静态事件1:", v));
    function staticEventHandler(v: number) { print("测试静态事件2:", v) }
    DelegateTest.onStaticEvent("add", staticEventHandler);
    DelegateTest.DipatchStaticEvent(123);
    DelegateTest.onStaticEvent("remove", staticEventHandler);
    DelegateTest.DipatchStaticEvent(123);

    try {
        // if (true) {
        if (IsStaticBinding()) {
            print("测试: 带 ref/out 的委托");
            actions.complexCall("add", (b, a, v) => {
                a.value += b;
                v.value = 999;
                return 789;
            });
            actions.TestComplexCall();
        } else {
            console.warn("reflectbind 模式不支持带 ref,out 参数的委托")
        }
    } catch (err) {
        console.error(err);
    }
}
