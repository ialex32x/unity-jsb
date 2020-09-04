
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

// 通过反射方式建立未导出类型的交互
let unknown = jsb.DelegateTest.GetNotExportedClass();
print(unknown.value);
print(unknown.GetType().value2);
print(unknown.Add(12, 21));
print("Equals(unknown, unknown):", System.Object.Equals(unknown, unknown));
print("Equals(unknown, camera):", System.Object.Equals(unknown, camera));
print("ReferenceEquals(unknown, unknown):", System.Object.ReferenceEquals(unknown, unknown));
print("ReferenceEquals(unknown, camera):", System.Object.ReferenceEquals(unknown, camera));
