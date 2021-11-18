"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const global_1 = require("global");
function execute() {
    if (module == require.main) {
        let g = { value: 1 };
        let x = {};
        let z = {};
        let y = 666;
        global_1.NoNamespaceClass.TestRefOut(g, x, y, z);
        console.assert(x.value == y * 1, "x");
        console.assert(z.value == y + 1, "z");
        console.assert(g.value == x.value + z.value, "g");
        print("ref/out:", g.value, x.value, z.value);
    }
    // a type description field for Out/Ref is optional but helpful to select the right overloaded method
    if (module == require.main) {
        let x = { type: Number };
        let z = { type: Number };
        let y = 233;
        global_1.NoNamespaceClass.TestOut(x, y, z);
        console.assert(x.value == y, "x == y ?");
        console.assert(z.value == y, "z == y ?");
        print("out:", x.value, typeof x.value, z.value, typeof z.value);
    }
    if (module == require.main) {
        let x = { type: String };
        let z = { type: String };
        let y = 999;
        global_1.NoNamespaceClass.TestOut(x, 999, z);
        console.assert(x.value == y.toString(), "x == y ?");
        console.assert(z.value == y.toString(), "z == y ?");
        print("out:", x.value, typeof x.value, z.value, typeof z.value);
    }
}
execute();
//# sourceMappingURL=example_ref_out.js.map