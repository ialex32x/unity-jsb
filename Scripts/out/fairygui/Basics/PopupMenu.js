"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class PopupMenu {
    static createInstance() {
        let inst = new PopupMenu();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "PopupMenu"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new PopupMenu();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.list = (this.gRoot.GetChildAt(1));
    }
}
exports.default = PopupMenu;
PopupMenu.URL = "ui://9leh0eyfl6f46x";
//# sourceMappingURL=PopupMenu.js.map