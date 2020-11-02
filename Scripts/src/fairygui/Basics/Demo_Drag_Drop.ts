/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class Demo_Drag_Drop {
	public gRoot: FairyGUI.GComponent

	public a: FairyGUI.GButton;
	public b: FairyGUI.GButton;
	public c: FairyGUI.GButton;
	public d: FairyGUI.GButton;
	public static URL: string = "ui://9leh0eyfgx2b78";

	public static createInstance(): Demo_Drag_Drop {
		let inst = new Demo_Drag_Drop();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Drag&Drop"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Drag_Drop {
		    let inst = new Demo_Drag_Drop();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.a = <FairyGUI.GButton>(this.gRoot.GetChildAt(0));
		this.b = <FairyGUI.GButton>(this.gRoot.GetChildAt(1));
		this.c = <FairyGUI.GButton>(this.gRoot.GetChildAt(2));
		this.d = <FairyGUI.GButton>(this.gRoot.GetChildAt(7));
	}
}