{
    console.log("string cache test:")
    let n = 100000;
    let start = 0;
    let end = 0;
    let str = "testing, testing, testing, testing, testing, testing, testing, testing";

    start = Date.now();
    for (let i = 0; i < n; i++) {
        jsb.ValueTest.Foo(str);
    }
    end = Date.now();
    console.log("time1:", (end - start) / 1000);

    jsb.AddCacheString(str);

    start = Date.now();
    for (let i = 0; i < n; i++) {
        jsb.ValueTest.Foo(str);
    }
    end = Date.now();
    console.log("time2:", (end - start) / 1000);
}
