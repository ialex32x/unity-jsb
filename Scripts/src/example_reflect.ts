
if (module == require.main) {
    let camera = UnityEngine.GameObject.Find("/Main Camera").GetComponent(UnityEngine.Camera);
    // 通过反射方式建立未导出类型的交互
    let unknown = jsb.DelegateTest.GetNotExportedClass();
    print(unknown.value);
    print(unknown.GetType().value2);
    print(unknown.Add(12, 21));
    print("Equals(unknown, unknown):", System.Object.Equals(unknown, unknown));
    print("Equals(unknown, camera):", System.Object.Equals(unknown, camera));
    print("ReferenceEquals(unknown, unknown):", System.Object.ReferenceEquals(unknown, unknown));
    print("ReferenceEquals(unknown, camera):", System.Object.ReferenceEquals(unknown, camera));
}
