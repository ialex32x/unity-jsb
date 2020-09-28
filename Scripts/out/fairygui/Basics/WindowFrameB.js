"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class WindowFrameB {
    static createInstance() {
        let inst = new WindowFrameB();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "WindowFrameB"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new WindowFrameB();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.dragArea = (this.gRoot.GetChildAt(1));
        this.closeButton = (this.gRoot.GetChildAt(2));
    }
}
exports.default = WindowFrameB;
WindowFrameB.URL = "ui://9leh0eyfniii7d";
//# sourceMappingURL=WindowFrameB.js.map