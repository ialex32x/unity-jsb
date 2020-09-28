"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class Dropdown2 {
    static createInstance() {
        let inst = new Dropdown2();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Dropdown2"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Dropdown2();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.list = (this.gRoot.GetChildAt(1));
    }
}
exports.default = Dropdown2;
Dropdown2.URL = "ui://9leh0eyfzd9g47";
//# sourceMappingURL=Dropdown2.js.map