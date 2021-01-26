let xhr = new XMLHttpRequest();
// xhr.open("GET", "http://127.0.0.1:8080/windows/checksum.txt");
xhr.open("GET", "https://www.baidu.com");
xhr.timeout = 3000;
xhr.onerror = function (e) {
    console.error(e);
};
xhr.onreadystatechange = function () {
    console.log("readyState:", xhr.readyState);
    if (xhr.readyState !== 4) {
        return;
    }
    console.log("status:", xhr.status);
    if (xhr.status == 200) {
        console.log("responseText:", xhr.responseText);
    }
};
xhr.send();
console.log("after send");
//# sourceMappingURL=example_xhr.js.map