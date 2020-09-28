"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class GridItem {
    static createInstance() {
        let inst = new GridItem();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "GridItem"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new GridItem();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.t0 = (this.gRoot.GetChildAt(2));
        this.t1 = (this.gRoot.GetChildAt(4));
        this.t2 = (this.gRoot.GetChildAt(5));
        this.star = (this.gRoot.GetChildAt(6));
    }
}
exports.default = GridItem;
GridItem.URL = "ui://9leh0eyfa7vt7n";
//# sourceMappingURL=GridItem.js.map