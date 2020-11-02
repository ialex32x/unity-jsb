"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Button1 {
    static createInstance() {
        let inst = new Button1();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Cooldown", "Button1"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Button1();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.mask = (this.gRoot.GetChildAt(3));
    }
}
exports.default = Button1;
Button1.URL = "ui://y768eypfltiql";
//# sourceMappingURL=Button1.js.map