
import { read, utils } from "xlsx";

let filename = "Assets/Examples/Data/test.xlsx";

if (typeof jsb === "undefined") {
    const fs = require("fs");
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

    console.log("read excel:", filename);
    for (var sheetIndex in wb.SheetNames) {
        var sheetName = wb.SheetNames[sheetIndex]
        
        console.log(`read sheet: ${sheetName}`);
        var sheet = wb.Sheets[sheetName];
        var range = utils.decode_range(sheet["!ref"]);
        for (var row = range.s.r; row <= range.e.r; row++) {
            for (var col = range.s.c; col <= range.e.c; col++) {
                var cell = sheet[utils.encode_cell({c: col, r: row})];
                if (cell) {
                    console.log(cell.v);
                }
            }
        }
    }    
}