if (module == require.main) {
    let g = { value: 1 };
    let x = {};
    let z = {};
    NoNamespaceClass.TestRefOut(g, x, 666, z);
    print("ref/out:", g.value, x.value, z.value);
}
// 可以通过指定 type 提高重载匹配的有效性
if (module == require.main) {
    let x = { type: Number };
    let z = { type: Number };
    NoNamespaceClass.TestOut(x, 233, z);
    print("out:", x.value, typeof x.value, z.value, typeof z.value);
}
if (module == require.main) {
    let x = { type: String };
    let z = { type: String };
    NoNamespaceClass.TestOut(x, 999, z);
    print("out:", x.value, typeof x.value, z.value, typeof z.value);
}
//# sourceMappingURL=example_ref_out.js.map