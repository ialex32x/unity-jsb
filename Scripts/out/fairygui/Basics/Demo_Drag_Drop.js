"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Demo_Drag_Drop {
    static createInstance() {
        let inst = new Demo_Drag_Drop();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Drag&Drop"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Drag_Drop();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.a = (this.gRoot.GetChildAt(0));
        this.b = (this.gRoot.GetChildAt(1));
        this.c = (this.gRoot.GetChildAt(2));
        this.d = (this.gRoot.GetChildAt(7));
    }
}
exports.default = Demo_Drag_Drop;
Demo_Drag_Drop.URL = "ui://9leh0eyfgx2b78";
//# sourceMappingURL=Demo_Drag_Drop.js.map