import BundleUsageMain from "./fairygui/BundleUsage/Main";
import CooldownMain from "./fairygui/Cooldown/Main";
import BasicsMain from "./fairygui/Basics/Main";
import { GameObject } from "UnityEngine";
import { GRoot, RelationType, UIContentScaler, UIPackage } from "FairyGUI";

if (module == require.main) {
    let go = new GameObject("FairyGUI");
    let scaler = go.AddComponent(UIContentScaler);

    scaler.designResolutionX = 1200;
    scaler.designResolutionY = 800;
    scaler.scaleMode = UIContentScaler.ScaleMode.ScaleWithScreenSize;
    // scaler.screenMatchMode = UIContentScaler.ScreenMatchMode.MatchWidthOrHeight;

    if (false) {
        UIPackage.AddPackage("UI/BundleUsage");
        let mainView = BundleUsageMain.createInstance();
        mainView.gRoot.fairyBatching = true;
        mainView.gRoot.SetSize(GRoot.inst.width, GRoot.inst.height);
        mainView.gRoot.AddRelation(GRoot.inst, RelationType.Size);
        GRoot.inst.AddChild(mainView.gRoot);
        mainView.t0.Play();
        mainView.theLabel.text = "Hello, Unity-jsb";
    }

    if (true) {
        UIPackage.AddPackage("UI/Basics");
        let mainView = BasicsMain.createInstance();
        mainView.gRoot.fairyBatching = true;
        mainView.gRoot.SetSize(GRoot.inst.width, GRoot.inst.height);
        mainView.gRoot.AddRelation(GRoot.inst, RelationType.Size);
        GRoot.inst.AddChild(mainView.gRoot);
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
