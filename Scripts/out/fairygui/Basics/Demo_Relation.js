"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class Demo_Relation {
    static createInstance() {
        let inst = new Demo_Relation();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Relation"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Relation();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.c1 = this.gRoot.GetControllerAt(0);
    }
}
exports.default = Demo_Relation;
Demo_Relation.URL = "ui://9leh0eyfwa8u2x";
//# sourceMappingURL=Demo_Relation.js.map