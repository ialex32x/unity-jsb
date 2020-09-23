
if (module == require.main) {
    let go = new UnityEngine.GameObject("test");
    console.log(go.name);
    go.name = "testing";
    console.log(go.name);

    async function destroy() {
        await jsb.Yield(new UnityEngine.WaitForSeconds(5));
        UnityEngine.Object.Destroy(go);
    }
    destroy();

    let camera = UnityEngine.GameObject.Find("/Main Camera").GetComponent(UnityEngine.Camera);
    let arr = camera.GetComponents(UnityEngine.Camera);
    print("array.length:", arr.length);
    print("array[0]:", arr[0] == camera);

    console.log("camera.name:", camera.name);
}
