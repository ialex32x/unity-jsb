"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const jsb_1 = require("jsb");
const UnityEngine_1 = require("UnityEngine");
const sample_monobehaviour_1 = require("./components/sample_monobehaviour");
if (module == require.main) {
    let go = new UnityEngine_1.GameObject("test");
    console.log(go.name);
    go.name = "testing";
    console.log(go.name);
    async function destroy() {
        await jsb_1.Yield(new UnityEngine_1.WaitForSeconds(5));
        UnityEngine_1.Object.Destroy(go);
        //! 机制原因, 无法直接利用 Object == 重载, 可以用 op_Implicit 判定 UnityEngine.Object 是否被销毁
        console.log("after destroy, go == null?", UnityEngine_1.Object.op_Implicit(go));
    }
    destroy();
    let camera = UnityEngine_1.GameObject.Find("/Main Camera").GetComponent(UnityEngine_1.Camera);
    let arr = camera.GetComponents(UnityEngine_1.Camera);
    print("array.length:", arr.length);
    print("array[0]:", arr[0] == camera);
    console.log("camera.name:", camera.name);
    UnityEngine_1.Debug.LogWarningFormat("blablabla... {0}", 123);
    let testPrefab = UnityEngine_1.Resources.Load("prefab/test");
    if (testPrefab) {
        let testGameObject = UnityEngine_1.Object.Instantiate(testPrefab);
        let testMyClass = testGameObject.GetComponent(sample_monobehaviour_1.MyClass);
        console.log(testMyClass);
    }
    else {
        console.warn("Resources/prefab/test.prefab not found");
    }
}
//# sourceMappingURL=example_unity.js.map