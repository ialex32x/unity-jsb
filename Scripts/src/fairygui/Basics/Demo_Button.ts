/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class Demo_Button {
	public gRoot: FairyGUI.GComponent

	public RadioGroup: FairyGUI.Controller;
	public tab: FairyGUI.Controller;
	public static URL: string = "ui://9leh0eyfrpmb1b";

	public static createInstance(): Demo_Button {
		let inst = new Demo_Button();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Button"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Button {
		    let inst = new Demo_Button();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.RadioGroup = this.gRoot.GetControllerAt(0);
		this.tab = this.gRoot.GetControllerAt(1);
	}
}