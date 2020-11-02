"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Button5 {
    static createInstance() {
        let inst = new Button5();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Button5"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Button5();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.bg = (this.gRoot.GetChildAt(0));
    }
}
exports.default = Button5;
Button5.URL = "ui://9leh0eyfrpmb13";
//# sourceMappingURL=Button5.js.map