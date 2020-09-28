"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
const WindowFrameB_1 = require("./WindowFrameB");
class WindowB {
    static createInstance() {
        let inst = new WindowB();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "WindowB"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new WindowB();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.frame = WindowFrameB_1.default.fromInstance(this.gRoot.GetChildAt(0));
        this.t1 = this.gRoot.GetTransitionAt(0);
    }
}
exports.default = WindowB;
WindowB.URL = "ui://9leh0eyf796x7a";
//# sourceMappingURL=WindowB.js.map