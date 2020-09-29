
if (module == require.main) {
    var takeBuffer = NoNamespaceClass.MakeBytes();
    var testBuffer = new Uint8Array(jsb.ToArrayBuffer(takeBuffer));
    var restoreBytes = jsb.ToBytes(testBuffer);
    var backBuffer = new Uint8Array(jsb.ToArrayBuffer(NoNamespaceClass.TestBytes(restoreBytes)));

    print("byte[] 处理");
    backBuffer.forEach(val => print(val));
}