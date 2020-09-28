"use strict";
/*
开发过程中可以使用 Default FileSystem 正常依赖 node_modules 中的库时
当需要发布到 Resources/Assetbundle 时, 这种情况下需要使用 webpack 等打包工具将脚本依赖全部打包, 即可在最终环境中使用第三方库
*/
Object.defineProperty(exports, "__esModule", { value: true });
const aw = require("../protogen/pb.bundle");
/*
具体参见 protobufjs 文档
npm: https://www.npmjs.com/package/protobufjs
github: https://github.com/protobufjs/protobuf.js

pbjs -t static-module -w commonjs -o pb.bundle.js awesome.proto
pbts -o pb.bundle.d.ts pb.bundle.js
*/
console.log("please run 'npm install' at first if 'protobufjs' module can not be resolved");
// 预生成代码的方式使用 proto 定义
let msg = aw.awesomepackage.AwesomeMessage.create({
    awesomeField: "hello, protobufjs",
});
console.log(msg.awesomeField);
let data = aw.awesomepackage.AwesomeMessage.encode(msg).finish();
console.log("encoded data (len):", data.byteLength);
let decMsg = aw.awesomepackage.AwesomeMessage.decode(data);
console.log("decoded message:", decMsg.awesomeField);
//# sourceMappingURL=example_pbjs_static.js.map