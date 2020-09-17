
//NOTE: 此处 xlsx 在 package.json 中以 devDependencies 方式依赖, 仅开发环境有效. 
//      可以改为 dev, 自行按需调整.
//      如运行报错, 请先确认是否已安装依赖包 (npm install 即可).

console.log("please run 'npm install' at first if 'xlsx' module can not be resolved");

import { read, utils } from "xlsx";

let filename = "Assets/Examples/Data/test.xlsx";

if (typeof UnityEngine === "undefined") {
    // 运行在 nodejs 环境

    const fs = require("fs");
    let data = fs.readFileSync(filename);
    // console.log(data);
    let wb = read(data);

    console.log(filename, typeof wb);
} else {
    // 运行在 Unity 环境

    let bytes = System.IO.File.ReadAllBytes(filename);
    let data = jsb.ToArrayBuffer(bytes);
    let wb = read(data, { type: "buffer" });

    console.log("read excel:", filename);
    for (var sheetIndex in wb.SheetNames) {
        var sheetName = wb.SheetNames[sheetIndex]

        console.log(`read sheet: ${sheetName}`);
        var sheet = wb.Sheets[sheetName];
        var range = utils.decode_range(sheet["!ref"]);
        for (var row = range.s.r; row <= range.e.r; row++) {
            for (var col = range.s.c; col <= range.e.c; col++) {
                var cell = sheet[utils.encode_cell({ c: col, r: row })];
                if (cell) {
                    console.log(cell.v);
                }
            }
        }
    }
}