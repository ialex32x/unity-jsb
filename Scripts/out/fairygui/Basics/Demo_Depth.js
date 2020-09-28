"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class Demo_Depth {
    static createInstance() {
        let inst = new Demo_Depth();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Depth"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Depth();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.btn0 = (this.gRoot.GetChildAt(2));
        this.btn1 = (this.gRoot.GetChildAt(3));
    }
}
exports.default = Demo_Depth;
Demo_Depth.URL = "ui://9leh0eyffou97q";
//# sourceMappingURL=Demo_Depth.js.map