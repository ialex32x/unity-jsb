"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class GridItem2 {
    static createInstance() {
        let inst = new GridItem2();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "GridItem2"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new GridItem2();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.t3 = (this.gRoot.GetChildAt(2));
        this.t1 = (this.gRoot.GetChildAt(4));
        this.cb = (this.gRoot.GetChildAt(5));
        this.mc = (this.gRoot.GetChildAt(6));
    }
}
exports.default = GridItem2;
GridItem2.URL = "ui://9leh0eyfatih7o";
//# sourceMappingURL=GridItem2.js.map