import * as pbjs from "protobufjs";

console.log("please run 'npm install' at first if 'protobufjs' module can not be resolved");

// 直接解析的方式载入 proto 定义
let awesome = System.IO.File.ReadAllText("Assets/Examples/Data/awesome.proto");
let pb = pbjs.parse(awesome);
let proto = pb.root.resolveAll();

for (let pb of proto.nestedArray) {
    console.log("read reflected protobuf message type:", pb);
}

