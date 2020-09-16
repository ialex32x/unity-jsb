
import { read } from "xlsx";

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
    let wb = read(data);

    console.log(filename, typeof wb);
} else {
    let bytes = System.IO.File.ReadAllBytes(filename);
    let data = jsb.ToArrayBuffer(bytes);
    let wb = read(data, {
        type: "buffer",
    });

    console.log(filename, wb);
}