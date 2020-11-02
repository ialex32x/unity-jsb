"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const jsb = require("jsb");
let HotfixTest = jsb.Import("HotfixTest");
try {
    // 反射导入的类型默认收到访问保护 (hotfix 后保护会被迫移除)
    print(HotfixTest.static_value);
}
catch (err) {
    console.warn("默认拒绝访问私有成员", err);
}
try {
    jsb.hotfix.replace_single("HotfixTest", ".ctor", function () {
        print("[HOTFIX][JS] 构造函数");
    });
    jsb.hotfix.replace_single("HotfixTest", "Foo", function (x) {
        print("[HOTFIX][JS] HotfixTest.Foo [private] this.value = ", this.value);
        return typeof x === "number" ? x + 3 : x + "~~~";
    });
    jsb.hotfix.before_single("HotfixTest", "AnotherStaticCall", function () {
        print("[HOTFIX][JS] HotfixTest.AnotherStaticCall 在 C# 执行前插入 JS 代码");
    });
    jsb.hotfix.replace_single("HotfixTest", "SimpleStaticCall", function () {
        this.AnotherStaticCall();
        print("[HOTFIX][JS] HotfixTest.SimpleStaticCall [private] this.static_value = ", this.static_value);
    });
}
catch (err) {
    console.warn("替换失败, 是否执行过dll注入?");
}
let hotfix = new HotfixTest();
print("[HOTFIX][JS] hotfix.Foo(1) 返回值:", hotfix.Foo(1));
print("[HOTFIX][JS] hotfix.Foo(1) 返回值:", hotfix.Foo("good day"));
HotfixTest.SimpleStaticCall();
//# sourceMappingURL=example_hotfix.js.map