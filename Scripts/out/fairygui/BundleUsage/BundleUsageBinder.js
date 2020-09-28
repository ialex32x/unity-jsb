"use strict";
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
Object.defineProperty(exports, "__esModule", { value: true });
const Main_1 = require("./Main");
class BundleUsageBinder {
    static bindAll() {
        FairyGUI.UIObjectFactory.SetPackageItemExtension(Main_1.default.URL, () => new Main_1.default());
    }
}
exports.default = BundleUsageBinder;
//# sourceMappingURL=BundleUsageBinder.js.map