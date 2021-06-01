
var express = require("express");
var path = require("path");
var morgan = require("morgan");
var parser = require("body-parser");
var multer = require("multer");
var pub_port = 8080;
var pub_path = "public";

for (var i = 2; i < process.argv.length;) {
    var arg = process.argv[i];
    if (arg == "--port") {
        pub_port = parseInt(process.argv[i + 1]);
        i += 2;
        continue;
    }
    if (arg == "--path") {
        pub_path = process.argv[i + 1];
        i += 2;
        continue;
    }
    i++;
}

var app = express();
app.set("view engine", "pug"); // 设置模板引擎
app.set("views", path.join(__dirname, "../render")); // 设置模板相对路径(相对当前目录)
app.use(morgan("dev"));
app.use(express.static(pub_path));
console.log("static files root:", pub_path);
app.get("/", function (req, res) {
    // req.path
    res.send("网站建设中...");
});
app.get("/render", function (req, res) {
    //console.log(req.param("name"))
    res.render("test", { title: "xxxTitlexxx", name: req.param("name") });
});
var storage = multer.diskStorage({
    destination: function (req, file, callback) {
        // console.log(file)
        callback(null, "./upload/");
    },
    filename: function (req, file, callback) {
        // console.log(file.originalname)
        callback(null, (new Date()).getTime() + "_" + file.originalname);
    }
});

var upload = multer({ storage: storage }).single("challenge");
app.post("/upload", function (req, res) {
    // console.log(req)
    upload(req, res, function (err) {
        if (err) {
            return res.end("Error:" + err);
        }
        return res.end("file uploaded.");
    });
});

app.use(function (req, res, next) {
    res.status(404).send("Page Not Found");
});

app.use(function (err, req, res, next) {
    console.error(err.stack);
    res.status(500).send("Broken");
});

app.use(parser.json());

app.listen(pub_port, function () {
    console.log("listening ... " + pub_port);
});
