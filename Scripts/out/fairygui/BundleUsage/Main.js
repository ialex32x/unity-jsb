"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
class Main {
    static createInstance() {
        let inst = new Main();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("BundleUsage", "Main"));
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.theLabel = (this.gRoot.GetChildAt(1));
        this.t0 = this.gRoot.GetTransitionAt(0);
    }
}
exports.default = Main;
Main.URL = "ui://d8m5tmokfou90";
//# sourceMappingURL=Main.js.map