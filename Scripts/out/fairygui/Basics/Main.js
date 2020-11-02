"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
const FairyGUI = require("FairyGUI");
class Main {
    static createInstance() {
        let inst = new Main();
        inst.gRoot = (FairyGUI.UIPackage.CreateObject("Basics", "Main"));
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
        this.c1 = this.gRoot.GetControllerAt(0);
        this.container = (this.gRoot.GetChildAt(1));
        this.btn_Back = (this.gRoot.GetChildAt(3));
        this.btn_Button = (this.gRoot.GetChildAt(4));
        this.btn_Image = (this.gRoot.GetChildAt(5));
        this.btn_Graph = (this.gRoot.GetChildAt(6));
        this.btn_MovieClip = (this.gRoot.GetChildAt(7));
        this.btn_Depth = (this.gRoot.GetChildAt(8));
        this.btn_Loader = (this.gRoot.GetChildAt(9));
        this.btn_List = (this.gRoot.GetChildAt(10));
        this.btn_ProgressBar = (this.gRoot.GetChildAt(11));
        this.btn_Slider = (this.gRoot.GetChildAt(12));
        this.btn_ComboBox = (this.gRoot.GetChildAt(13));
        this.btn_ClipAndScroll = (this.gRoot.GetChildAt(14));
        this.btn_Controller = (this.gRoot.GetChildAt(15));
        this.btn_Relation = (this.gRoot.GetChildAt(16));
        this.btn_Label = (this.gRoot.GetChildAt(17));
        this.btn_Popup = (this.gRoot.GetChildAt(18));
        this.btn_Window = (this.gRoot.GetChildAt(19));
        this.btn_DragAndDrop = (this.gRoot.GetChildAt(20));
        this.btn_Component = (this.gRoot.GetChildAt(21));
        this.btn_Grid = (this.gRoot.GetChildAt(22));
        this.btn_Text = (this.gRoot.GetChildAt(23));
        this.btns = (this.gRoot.GetChildAt(24));
    }
}
exports.default = Main;
Main.URL = "ui://9leh0eyfrpmb1c";
//# sourceMappingURL=Main.js.map