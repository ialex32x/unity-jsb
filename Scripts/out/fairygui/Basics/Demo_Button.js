"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Demo_Button {
    static createInstance() {
        let inst = new Demo_Button();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Button"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Button();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.RadioGroup = this.gRoot.GetControllerAt(0);
        this.tab = this.gRoot.GetControllerAt(1);
    }
}
exports.default = Demo_Button;
Demo_Button.URL = "ui://9leh0eyfrpmb1b";
//# sourceMappingURL=Demo_Button.js.map