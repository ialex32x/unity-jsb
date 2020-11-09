
/*
开发过程中可以使用 Default FileSystem 正常依赖 node_modules 中的库时
当需要发布到 Resources/Assetbundle 时, 这种情况下需要使用 webpack 等打包工具将脚本依赖全部打包, 即可在最终环境中使用第三方库
*/

//NOTE: 此处 xlsx 在 package.json 中以 devDependencies 方式依赖, 仅开发环境有效. 
//      可以改为 dev, 自行按需调整.
//      如运行报错, 请先确认是否已安装依赖包 (npm install 即可).

console.log("please run 'npm install' at first if 'xlsx' module can not be resolved");

import * as jsb from "jsb";
import { File } from "System.IO";
import * as xlsx from "xlsx";

let filename = "Assets/Examples/Data/test.xlsx";
let bytes = File.ReadAllBytes(filename);
let data = jsb.ToArrayBuffer(bytes);
let wb = xlsx.read(data, { type: "buffer" });

console.log("read excel:", filename);
for (var sheetIndex in wb.SheetNames) {
    var sheetName = wb.SheetNames[sheetIndex]

    console.log(`read sheet: ${sheetName}`);
    var sheet = wb.Sheets[sheetName];
    var range = xlsx.utils.decode_range(sheet["!ref"]);
    for (var row = range.s.r; row <= range.e.r; row++) {
        for (var col = range.s.c; col <= range.e.c; col++) {
            var cell = sheet[xlsx.utils.encode_cell({ c: col, r: row })];
            if (cell) {
                console.log(cell.v);
            }
        }
    }
}
