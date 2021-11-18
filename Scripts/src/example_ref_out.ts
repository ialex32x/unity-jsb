import { NoNamespaceClass } from "global";
import * as jsb from "jsb";

function execute() {
    if (module == require.main) {
        let g: jsb.Ref<number> = { value: 1 };
        let x: jsb.Out<number> = {};
        let z: jsb.Out<number> = {};
        let y = 666;
        NoNamespaceClass.TestRefOut(g, x, y, z);
        console.assert(x.value == y * 1, "x");
        console.assert(z.value == y + 1, "z");
        console.assert(g.value == x.value + z.value, "g");
        print("ref/out:", g.value, x.value, z.value);
    }

    // a type description field for Out/Ref is optional but helpful to select the right overloaded method

    if (module == require.main) {
        let x: jsb.Out<number> = { type: Number };
        let z: jsb.Out<number> = { type: Number };
        let y = 233;
        NoNamespaceClass.TestOut(x, y, z);
        console.assert(x.value == y, "x == y ?");
        console.assert(z.value == y, "z == y ?");
        print("out:", x.value, typeof x.value, z.value, typeof z.value);
    }

    if (module == require.main) {
        let x: jsb.Out<string> = { type: String };
        let z: jsb.Out<string> = { type: String };
        let y = 999;
        NoNamespaceClass.TestOut(x, 999, z);
        console.assert(x.value == y.toString(), "x == y ?");
        console.assert(z.value == y.toString(), "z == y ?");
        print("out:", x.value, typeof x.value, z.value, typeof z.value);
    }
}

execute();
