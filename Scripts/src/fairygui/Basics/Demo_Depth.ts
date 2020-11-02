/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class Demo_Depth {
	public gRoot: FairyGUI.GComponent

	public btn0: FairyGUI.GButton;
	public btn1: FairyGUI.GButton;
	public static URL: string = "ui://9leh0eyffou97q";

	public static createInstance(): Demo_Depth {
		let inst = new Demo_Depth();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Depth"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Depth {
		    let inst = new Demo_Depth();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.btn0 = <FairyGUI.GButton>(this.gRoot.GetChildAt(2));
		this.btn1 = <FairyGUI.GButton>(this.gRoot.GetChildAt(3));
	}
}