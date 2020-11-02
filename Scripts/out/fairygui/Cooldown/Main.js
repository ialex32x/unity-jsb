"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
const Button1_1 = require("./Button1");
const Button2_1 = require("./Button2");
class Main {
    static createInstance() {
        let inst = new Main();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Cooldown", "Main"));
        inst.onConstruct();
        return inst;
    }
    static fromInstance(gRoot) {
        let inst = new Main();
        inst.gRoot = gRoot;
        inst.onConstruct();
        return inst;
    }
    onConstruct() {
        this.b0 = Button1_1.default.fromInstance(this.gRoot.GetChildAt(0));
        this.b1 = Button2_1.default.fromInstance(this.gRoot.GetChildAt(1));
    }
}
exports.default = Main;
Main.URL = "ui://y768eypffvaib";
//# sourceMappingURL=Main.js.map