"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class Button52 {
    static createInstance() {
        let inst = new Button52();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Button52"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Button52();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.grayed = this.gRoot.GetControllerAt(1);
        this.bg = (this.gRoot.GetChildAt(0));
    }
}
exports.default = Button52;
Button52.URL = "ui://9leh0eyfdyz47i";
//# sourceMappingURL=Button52.js.map