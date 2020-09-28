"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class Demo_Graph {
    static createInstance() {
        let inst = new Demo_Graph();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Graph"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Graph();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.polygon = (this.gRoot.GetChildAt(5));
        this.polygon2 = (this.gRoot.GetChildAt(6));
        this.line = (this.gRoot.GetChildAt(7));
        this.line3 = (this.gRoot.GetChildAt(8));
        this.pie = (this.gRoot.GetChildAt(12));
        this.radial = (this.gRoot.GetChildAt(14));
        this.trapezoid = (this.gRoot.GetChildAt(15));
        this.line2 = (this.gRoot.GetChildAt(16));
    }
}
exports.default = Demo_Graph;
Demo_Graph.URL = "ui://9leh0eyfhixt1m";
//# sourceMappingURL=Demo_Graph.js.map