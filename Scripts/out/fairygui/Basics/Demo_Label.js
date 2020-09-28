"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
const WindowFrame_1 = require("./WindowFrame");
class Demo_Label {
    static createInstance() {
        let inst = new Demo_Label();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Label"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Label();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.frame = WindowFrame_1.default.fromInstance(this.gRoot.GetChildAt(1));
    }
}
exports.default = Demo_Label;
Demo_Label.URL = "ui://9leh0eyfw42o3j";
//# sourceMappingURL=Demo_Label.js.map