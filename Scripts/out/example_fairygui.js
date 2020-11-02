"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Main_1 = require("./fairygui/BundleUsage/Main");
const Main_2 = require("./fairygui/Basics/Main");
const UnityEngine_1 = require("UnityEngine");
const FairyGUI_1 = require("FairyGUI");
if (module == require.main) {
    let go = new UnityEngine_1.GameObject("FairyGUI");
    let scaler = go.AddComponent(FairyGUI_1.UIContentScaler);
    scaler.designResolutionX = 1200;
    scaler.designResolutionY = 800;
    scaler.scaleMode = FairyGUI_1.UIContentScaler.ScaleMode.ScaleWithScreenSize;
    // scaler.screenMatchMode = UIContentScaler.ScreenMatchMode.MatchWidthOrHeight;
    if (false) {
        FairyGUI_1.UIPackage.AddPackage("UI/BundleUsage");
        let mainView = Main_1.default.createInstance();
        mainView.gRoot.fairyBatching = true;
        mainView.gRoot.SetSize(FairyGUI_1.GRoot.inst.width, FairyGUI_1.GRoot.inst.height);
        mainView.gRoot.AddRelation(FairyGUI_1.GRoot.inst, FairyGUI_1.RelationType.Size);
        FairyGUI_1.GRoot.inst.AddChild(mainView.gRoot);
        mainView.t0.Play();
        mainView.theLabel.text = "Hello, Unity-jsb";
    }
    if (true) {
        FairyGUI_1.UIPackage.AddPackage("UI/Basics");
        let mainView = Main_2.default.createInstance();
        mainView.gRoot.fairyBatching = true;
        mainView.gRoot.SetSize(FairyGUI_1.GRoot.inst.width, FairyGUI_1.GRoot.inst.height);
        mainView.gRoot.AddRelation(FairyGUI_1.GRoot.inst, FairyGUI_1.RelationType.Size);
        FairyGUI_1.GRoot.inst.AddChild(mainView.gRoot);
        let fn = evt => {
            console.log(evt.type);
        };
        console.log("添加按钮回调");
        mainView.btn_Button.onClick.Add(fn);
        setTimeout(() => {
            console.log("移除按钮回调");
            mainView.btn_Button.onClick.Remove(fn);
        }, 30000);
    }
    // UIPackage.AddPackage("UI/Cooldown");
    // let mainView = CooldownMain.createInstance();
    // GRoot.inst.AddChild(mainView.gRoot);
    // mainView.b1.gRoot.text = "A";
    // let mainView = UIPackage.CreateObject("BundleUsage", "Main").asCom;
    // mainView.fairyBatching = true;
    // mainView.SetSize(GRoot.inst.width, GRoot.inst.height);
    // mainView.AddRelation(GRoot.inst, RelationType.Size);
    // GRoot.inst.AddChild(mainView);
    // mainView.GetTransition("t0").Play();
}
//# sourceMappingURL=example_fairygui.js.map