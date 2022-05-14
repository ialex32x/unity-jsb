"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Example_1 = require("Example");
const jsb = require("jsb");
/*
    jsb.AddCacheString is available to use to avoid unnecessary string allocations on frequently translating strings between Javascript and C#
    NOTE: remove string from cache with jsb.RemoveCacheString
*/
{
    console.log("string cache test:");
    let n = 100000;
    let start = 0;
    let end = 0;
    let str = "testing, testing, testing, testing, testing, testing, testing, testing";
    start = Date.now();
    for (let i = 0; i < n; i++) {
        Example_1.ValueTest.Foo(str);
    }
    end = Date.now();
    console.log("time1:", (end - start) / 1000);
    jsb.AddCacheString(str);
    start = Date.now();
    for (let i = 0; i < n; i++) {
        Example_1.ValueTest.Foo(str);
    }
    end = Date.now();
    console.log("time2:", (end - start) / 1000);
}
//# sourceMappingURL=example_string_cache.js.map