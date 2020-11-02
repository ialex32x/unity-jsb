import { NoNamespaceClass } from "global";
import { ToArrayBuffer, ToBytes } from "jsb";

if (module == require.main) {
    var takeBuffer = NoNamespaceClass.MakeBytes();
    var testBuffer = new Uint8Array(ToArrayBuffer(takeBuffer));
    var restoreBytes = ToBytes(testBuffer);
    var backBuffer = new Uint8Array(ToArrayBuffer(NoNamespaceClass.TestBytes(restoreBytes)));

    print("byte[] 处理");
    backBuffer.forEach(val => print(val));
}