import { Yield } from "jsb";
import { Camera, Debug, GameObject, Object, WaitForSeconds } from "UnityEngine";
import { Object as Object1 } from "System";

if (module == require.main) {
    let go = new GameObject("test");
    console.log(go.name);
    go.name = "testing";
    console.log(go.name);

    async function destroy() {
        await Yield(new WaitForSeconds(5));
        Object.Destroy(go);
    }
    destroy();

    let camera = GameObject.Find("/Main Camera").GetComponent(Camera);
    let arr = camera.GetComponents(Camera);
    print("array.length:", arr.length);
    print("array[0]:", arr[0] == camera);

    console.log("camera.name:", camera.name);
    Debug.LogWarningFormat("blablabla... {0}", <Object1><any>123);
}
