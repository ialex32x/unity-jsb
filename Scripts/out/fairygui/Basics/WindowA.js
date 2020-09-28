"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
const WindowFrame_1 = require("./WindowFrame");
class WindowA {
    static createInstance() {
        let inst = new WindowA();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "WindowA"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new WindowA();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.frame = WindowFrame_1.default.fromInstance(this.gRoot.GetChildAt(0));
    }
}
exports.default = WindowA;
WindowA.URL = "ui://9leh0eyfl6f473";
//# sourceMappingURL=WindowA.js.map