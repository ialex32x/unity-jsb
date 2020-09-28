import Main from "./fairygui/BundleUsage/Main";
import CooldownMain from "./fairygui/Cooldown/Main";

if (module == require.main) {
    let go = new UnityEngine.GameObject("FairyGUI");
    let scaler = go.AddComponent(FairyGUI.UIContentScaler);

    scaler.designResolutionX = 1200;
    scaler.designResolutionY = 800;
    scaler.scaleMode = FairyGUI.UIContentScaler.ScaleMode.ScaleWithScreenSize;
    // scaler.screenMatchMode = FairyGUI.UIContentScaler.ScreenMatchMode.MatchWidthOrHeight;

    FairyGUI.UIPackage.AddPackage("UI/BundleUsage");
    var mainView = Main.createInstance();

    mainView.gRoot.fairyBatching = true;
    mainView.gRoot.SetSize(FairyGUI.GRoot.inst.width, FairyGUI.GRoot.inst.height);
    mainView.gRoot.AddRelation(FairyGUI.GRoot.inst, FairyGUI.RelationType.Size);
    FairyGUI.GRoot.inst.AddChild(mainView.gRoot);
    mainView.t0.Play();
    mainView.theLabel.text = "Hello, Unity-jsb";
    
    // FairyGUI.UIPackage.AddPackage("UI/Cooldown");
    // var mainView = CooldownMain.createInstance();
    // FairyGUI.GRoot.inst.AddChild(mainView.gRoot);
    // mainView.b1.gRoot.text = "A";

    // let mainView = FairyGUI.UIPackage.CreateObject("BundleUsage", "Main").asCom;
    // mainView.fairyBatching = true;
    // mainView.SetSize(FairyGUI.GRoot.inst.width, FairyGUI.GRoot.inst.height);
    // mainView.AddRelation(FairyGUI.GRoot.inst, FairyGUI.RelationType.Size);

    // FairyGUI.GRoot.inst.AddChild(mainView);
    // mainView.GetTransition("t0").Play();
}
