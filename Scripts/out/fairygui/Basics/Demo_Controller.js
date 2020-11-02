"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Demo_Controller {
    static createInstance() {
        let inst = new Demo_Controller();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Demo_Controller"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Demo_Controller();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.c1 = this.gRoot.GetControllerAt(0);
        this.c2 = this.gRoot.GetControllerAt(1);
        this.switchBtn = (this.gRoot.GetChildAt(13));
    }
}
exports.default = Demo_Controller;
Demo_Controller.URL = "ui://9leh0eyfwa8u2v";
//# sourceMappingURL=Demo_Controller.js.map