"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class WindowFrame {
    static createInstance() {
        let inst = new WindowFrame();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "WindowFrame"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new WindowFrame();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.closeButton = (this.gRoot.GetChildAt(1));
        this.dragArea = (this.gRoot.GetChildAt(2));
        this.contentArea = (this.gRoot.GetChildAt(4));
    }
}
exports.default = WindowFrame;
WindowFrame.URL = "ui://9leh0eyfrt103l";
//# sourceMappingURL=WindowFrame.js.map