"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Main_1 = require("./fairygui/BundleUsage/Main");
const Main_2 = require("./fairygui/Basics/Main");
if (module == require.main) {
    let go = new UnityEngine.GameObject("FairyGUI");
    let scaler = go.AddComponent(FairyGUI.UIContentScaler);
    scaler.designResolutionX = 1200;
    scaler.designResolutionY = 800;
    scaler.scaleMode = FairyGUI.UIContentScaler.ScaleMode.ScaleWithScreenSize;
    // scaler.screenMatchMode = FairyGUI.UIContentScaler.ScreenMatchMode.MatchWidthOrHeight;
    if (false) {
        FairyGUI.UIPackage.AddPackage("UI/BundleUsage");
        let mainView = Main_1.default.createInstance();
        mainView.gRoot.fairyBatching = true;
        mainView.gRoot.SetSize(FairyGUI.GRoot.inst.width, FairyGUI.GRoot.inst.height);
        mainView.gRoot.AddRelation(FairyGUI.GRoot.inst, FairyGUI.RelationType.Size);
        FairyGUI.GRoot.inst.AddChild(mainView.gRoot);
        mainView.t0.Play();
        mainView.theLabel.text = "Hello, Unity-jsb";
    }
    if (true) {
        FairyGUI.UIPackage.AddPackage("UI/Basics");
        let mainView = Main_2.default.createInstance();
        mainView.gRoot.fairyBatching = true;
        mainView.gRoot.SetSize(FairyGUI.GRoot.inst.width, FairyGUI.GRoot.inst.height);
        mainView.gRoot.AddRelation(FairyGUI.GRoot.inst, FairyGUI.RelationType.Size);
        FairyGUI.GRoot.inst.AddChild(mainView.gRoot);
        mainView.btn_Button.onClick.Add(evt => {
            console.log(evt.type);
        });
    }
    // FairyGUI.UIPackage.AddPackage("UI/Cooldown");
    // let mainView = CooldownMain.createInstance();
    // FairyGUI.GRoot.inst.AddChild(mainView.gRoot);
    // mainView.b1.gRoot.text = "A";
    // let mainView = FairyGUI.UIPackage.CreateObject("BundleUsage", "Main").asCom;
    // mainView.fairyBatching = true;
    // mainView.SetSize(FairyGUI.GRoot.inst.width, FairyGUI.GRoot.inst.height);
    // mainView.AddRelation(FairyGUI.GRoot.inst, FairyGUI.RelationType.Size);
    // FairyGUI.GRoot.inst.AddChild(mainView);
    // mainView.GetTransition("t0").Play();
}
//# sourceMappingURL=example_fairygui.js.map