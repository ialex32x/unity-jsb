"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const global_1 = require("global");
const jsb_1 = require("jsb");
if (module == require.main) {
    var takeBuffer = global_1.NoNamespaceClass.MakeBytes();
    var testBuffer = new Uint8Array(jsb_1.ToArrayBuffer(takeBuffer));
    var restoreBytes = jsb_1.ToBytes(testBuffer);
    var backBuffer = new Uint8Array(jsb_1.ToArrayBuffer(global_1.NoNamespaceClass.TestBytes(restoreBytes)));
    print("byte[] 处理");
    backBuffer.forEach(val => print(val));
}
//# sourceMappingURL=example_bytes.js.map