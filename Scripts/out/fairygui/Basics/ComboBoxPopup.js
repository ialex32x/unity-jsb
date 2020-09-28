"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class ComboBoxPopup {
    static createInstance() {
        let inst = new ComboBoxPopup();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "ComboBoxPopup"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new ComboBoxPopup();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.list = (this.gRoot.GetChildAt(1));
    }
}
exports.default = ComboBoxPopup;
ComboBoxPopup.URL = "ui://9leh0eyfrt103y";
//# sourceMappingURL=ComboBoxPopup.js.map