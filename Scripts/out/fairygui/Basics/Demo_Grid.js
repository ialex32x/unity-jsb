"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Demo_Grid {
    static createInstance() {
        let inst = new Demo_Grid();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Grid"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Grid();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.list1 = (this.gRoot.GetChildAt(2));
        this.list2 = (this.gRoot.GetChildAt(10));
    }
}
exports.default = Demo_Grid;
Demo_Grid.URL = "ui://9leh0eyfc2z47l";
//# sourceMappingURL=Demo_Grid.js.map