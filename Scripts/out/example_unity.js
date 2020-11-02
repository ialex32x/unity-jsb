"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const jsb_1 = require("jsb");
const UnityEngine_1 = require("UnityEngine");
if (module == require.main) {
    let go = new UnityEngine_1.GameObject("test");
    console.log(go.name);
    go.name = "testing";
    console.log(go.name);
    async function destroy() {
        await jsb_1.Yield(new UnityEngine_1.WaitForSeconds(5));
        UnityEngine_1.Object.Destroy(go);
    }
    destroy();
    let camera = UnityEngine_1.GameObject.Find("/Main Camera").GetComponent(UnityEngine_1.Camera);
    let arr = camera.GetComponents(UnityEngine_1.Camera);
    print("array.length:", arr.length);
    print("array[0]:", arr[0] == camera);
    console.log("camera.name:", camera.name);
    UnityEngine_1.Debug.LogWarningFormat("blablabla... {0}", 123);
}
//# sourceMappingURL=example_unity.js.map