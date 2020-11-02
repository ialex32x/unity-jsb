"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Example_1 = require("Example");
const UnityEngine_1 = require("UnityEngine");
const System_1 = require("System");
if (module == require.main) {
    let camera = UnityEngine_1.GameObject.Find("/Main Camera").GetComponent(UnityEngine_1.Camera);
    // 通过反射方式建立未导出类型的交互
    let unknown = Example_1.DelegateTest.GetNotExportedClass();
    print(unknown.value);
    print(unknown.GetType().value2);
    print(unknown.Add(12, 21));
    print("Equals(unknown, unknown):", System_1.Object.Equals(unknown, unknown));
    print("Equals(unknown, camera):", System_1.Object.Equals(unknown, camera));
    print("ReferenceEquals(unknown, unknown):", System_1.Object.ReferenceEquals(unknown, unknown));
    print("ReferenceEquals(unknown, camera):", System_1.Object.ReferenceEquals(unknown, camera));
}
//# sourceMappingURL=example_reflect.js.map