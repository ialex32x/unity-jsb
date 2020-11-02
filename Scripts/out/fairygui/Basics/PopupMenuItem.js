"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class PopupMenuItem {
    static createInstance() {
        let inst = new PopupMenuItem();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "PopupMenuItem"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new PopupMenuItem();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.checked = this.gRoot.GetControllerAt(1);
    }
}
exports.default = PopupMenuItem;
PopupMenuItem.URL = "ui://9leh0eyfl6f46z";
//# sourceMappingURL=PopupMenuItem.js.map