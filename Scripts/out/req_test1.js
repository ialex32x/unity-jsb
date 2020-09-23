var e2 = require("./req_test2");
exports.test = "hello: " + e2.test;
console.log("[req_test1] ", exports.test, __dirname, __filename);
console.log("req_test, isMain?", module == require.main);
//# sourceMappingURL=req_test1.js.map