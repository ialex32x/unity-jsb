/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class PopupMenuItem {
	public gRoot: FairyGUI.GButton

	public checked: FairyGUI.Controller;
	public static URL: string = "ui://9leh0eyfl6f46z";

	public static createInstance(): PopupMenuItem {
		let inst = new PopupMenuItem();
		inst.gRoot = <FairyGUI.GButton>(FairyGUI.UIPackage.CreateObject("Basics", "PopupMenuItem"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): PopupMenuItem {
		    let inst = new PopupMenuItem();
		    inst.gRoot = <FairyGUI.GButton>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.checked = this.gRoot.GetControllerAt(1);
	}
}