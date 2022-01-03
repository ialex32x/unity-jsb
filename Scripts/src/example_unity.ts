import { Yield } from "jsb";
import { Camera, Debug, GameObject, Object, Resources, WaitForSeconds } from "UnityEngine";
import { Object as Object1 } from "System";
import { MyClass } from "./components/sample_monobehaviour";

if (module == require.main) {
    let go = new GameObject("test");
    console.log(go.name);
    go.name = "testing";
    console.log(go.name);

    async function destroy() {
        await Yield(new WaitForSeconds(5));
        Object.Destroy(go);

        //! 机制原因, 无法直接利用 Object == 重载, 可以用 op_Implicit 判定 UnityEngine.Object 是否被销毁
        console.log("after destroy, go == null?", Object.op_Implicit(go));
    }
    destroy();

    let camera = GameObject.Find("/Main Camera").GetComponent(Camera);
    let arr = camera.GetComponents(Camera);
    print("array.length:", arr.length);
    print("array[0]:", arr[0] == camera);

    console.log("camera.name:", camera.name);
    Debug.LogWarningFormat("blablabla... {0}", <Object1><any>123);

    let testPrefab = Resources.Load("prefab/test");
    if (testPrefab) {
        let testGameObject = <GameObject>Object.Instantiate(testPrefab);
        let testMyClass = testGameObject.GetComponent(MyClass);

        console.log(testMyClass);
    } else {
        console.warn("Resources/prefab/test.prefab not found");
    }
}
