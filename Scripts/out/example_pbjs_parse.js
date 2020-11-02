"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const pbjs = require("protobufjs");
const System_IO_1 = require("System.IO");
console.log("please run 'npm install' at first if 'protobufjs' module can not be resolved");
// 直接解析的方式载入 proto 定义
let awesome = System_IO_1.File.ReadAllText("Assets/Examples/Data/awesome.proto");
let pb = pbjs.parse(awesome);
let proto = pb.root.resolveAll();
for (let pb of proto.nestedArray) {
    console.log("read reflected protobuf message type:", pb);
    let nestedArray = pb.nestedArray;
    if (typeof nestedArray !== "undefined") {
        for (let pb_nested of nestedArray) {
            console.log("pb_nested:", pb_nested);
        }
    }
}
let type = proto.lookupType("awesomepackage.AwesomeMessage");
let msg = type.fromObject({
    awesomeField: "hello, protobufjs",
});
let data = type.encode(msg).finish();
console.log("encoded data (len):", data.byteLength);
let decMsg = type.decode(data);
console.log("decoded message:", decMsg["awesomeField"]);
//# sourceMappingURL=example_pbjs_parse.js.map