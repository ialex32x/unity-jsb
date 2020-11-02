import { DelegateTest } from "Example";
import { Camera, GameObject } from "UnityEngine";
import { Object } from "System";

if (module == require.main) {
    let camera = GameObject.Find("/Main Camera").GetComponent(Camera);
    // 通过反射方式建立未导出类型的交互
    let unknown = DelegateTest.GetNotExportedClass();
    print(unknown.value);
    print(unknown.GetType().value2);
    print(unknown.Add(12, 21));
    print("Equals(unknown, unknown):", Object.Equals(unknown, unknown));
    print("Equals(unknown, camera):", Object.Equals(unknown, camera));
    print("ReferenceEquals(unknown, unknown):", Object.ReferenceEquals(unknown, unknown));
    print("ReferenceEquals(unknown, camera):", Object.ReferenceEquals(unknown, camera));
}
