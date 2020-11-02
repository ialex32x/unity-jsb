/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class Button52 {
	public gRoot: FairyGUI.GButton

	public grayed: FairyGUI.Controller;
	public bg: FairyGUI.GImage;
	public static URL: string = "ui://9leh0eyfdyz47i";

	public static createInstance(): Button52 {
		let inst = new Button52();
		inst.gRoot = <FairyGUI.GButton>(FairyGUI.UIPackage.CreateObject("Basics", "Button52"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Button52 {
		    let inst = new Button52();
		    inst.gRoot = <FairyGUI.GButton>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.grayed = this.gRoot.GetControllerAt(1);
		this.bg = <FairyGUI.GImage>(this.gRoot.GetChildAt(0));
	}
}