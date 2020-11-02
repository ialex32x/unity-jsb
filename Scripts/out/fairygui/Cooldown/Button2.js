"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Button2 {
    static createInstance() {
        let inst = new Button2();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Cooldown", "Button2"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Button2();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.btn = (this.gRoot.GetChildAt(3));
        this.mask = (this.gRoot.GetChildAt(5));
    }
}
exports.default = Button2;
Button2.URL = "ui://y768eypfp3yap";
//# sourceMappingURL=Button2.js.map