let System = require("System");

console.log("hello", System.Math.Abs(-123));
System.Console.WriteLine("test");

let start = Date.now();
let a = 0; for (let i = 0; i < 10000000; i++) { a++; a *= 2; a /= 3; }
let end = Date.now();

console.log((end - start) / 1000);

