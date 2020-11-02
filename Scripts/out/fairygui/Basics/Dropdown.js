"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Dropdown {
    static createInstance() {
        let inst = new Dropdown();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Dropdown"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Dropdown();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.button = (this.gRoot.GetChildAt(0));
    }
}
exports.default = Dropdown;
Dropdown.URL = "ui://9leh0eyfzd9g41";
//# sourceMappingURL=Dropdown.js.map