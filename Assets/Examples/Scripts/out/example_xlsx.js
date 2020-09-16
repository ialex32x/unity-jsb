"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const xlsx_1 = require("xlsx");
// let b = [];
// b[4] = 5;
// console.log(b[4], b.length);
// let a = new Uint16Array(4);
// console.log(a[4]);
// a[4] = 5;
// console.log(a[4]);
let filename = "Assets/Examples/Data/test.xlsx";
if (typeof jsb === "undefined") {
    const fs = require("fs");
    const b = Buffer.from("");
    let data = fs.readFileSync(filename);
    // console.log(data);
    let wb = xlsx_1.read(data);
    console.log(filename, typeof wb);
}
else {
    let bytes = System.IO.File.ReadAllBytes(filename);
    let data = jsb.ToArrayBuffer(bytes);
    let wb = xlsx_1.read(data, {
        type: "buffer",
    });
    console.log(filename, wb);
}
//# sourceMappingURL=example_xlsx.js.map