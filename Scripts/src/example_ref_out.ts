
if (module == require.main) {
    let g: jsb.Ref<number> = { value: 1 };
    let x: jsb.Out<number> = {};
    let z: jsb.Out<number> = {};
    NoNamespaceClass.TestRefOut(g, x, 666, z);
    print("ref/out:", g.value, x.value, z.value);
}

// 可以通过指定 type 提高重载匹配的有效性

if (module == require.main) {
    let x: jsb.Out<number> = { type: Number };
    let z: jsb.Out<number> = { type: Number };
    NoNamespaceClass.TestOut(x, 233, z);
    print("out:", x.value, typeof x.value, z.value, typeof z.value);
}

if (module == require.main) {
    let x: jsb.Out<string> = { type: String };
    let z: jsb.Out<string> = { type: String };
    NoNamespaceClass.TestOut(x, 999, z);
    print("out:", x.value, typeof x.value, z.value, typeof z.value);
}
