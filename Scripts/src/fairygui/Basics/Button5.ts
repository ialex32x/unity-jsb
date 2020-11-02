/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class Button5 {
	public gRoot: FairyGUI.GButton

	public bg: FairyGUI.GImage;
	public static URL: string = "ui://9leh0eyfrpmb13";

	public static createInstance(): Button5 {
		let inst = new Button5();
		inst.gRoot = <FairyGUI.GButton>(FairyGUI.UIPackage.CreateObject("Basics", "Button5"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Button5 {
		    let inst = new Button5();
		    inst.gRoot = <FairyGUI.GButton>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.bg = <FairyGUI.GImage>(this.gRoot.GetChildAt(0));
	}
}