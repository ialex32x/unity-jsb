"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
const jsb = require("jsb");
let _id = 0;
function allocId() {
    return ++_id;
}
let all = 600;
let act = 0;
for (let i = 0; i < all; i++) {
    let n = i;
    let d1 = (n * 10 * Math.floor(UnityEngine_1.Random.Range(100, 10000))) % (3 * 60 * 1000);
    let e1 = d1 + jsb.Now();
    setTimeout(t1 => {
        if (n % 10 == 0) {
            all++;
            let d2 = Math.floor(UnityEngine_1.Random.Range(1000 * 1, 1000 * 60 * 2));
            let e2 = d2 + jsb.Now();
            setTimeout(t2 => {
                act++;
                let n2 = jsb.Now();
                let x2 = n2 - e2;
                console.log(`    > ${n} ${t2.id} => Now: ${n2} Exp: ${e2} Delay: ${d2} Delta: ${x2} Act: ${act}/${all}`);
            }, d2, { id: allocId() });
        }
        act++;
        let n1 = jsb.Now();
        let x1 = n1 - e1;
        console.log(`TA ${n} ${t1.id} => Now: ${n1} Exp: ${e1} Delay: ${d1} Delta: ${x1} Act: ${act}/${all}`);
    }, d1, { id: allocId() });
}
//# sourceMappingURL=example_timer.js.map