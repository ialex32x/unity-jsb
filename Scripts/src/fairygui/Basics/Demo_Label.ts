/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
import WindowFrame from "./WindowFrame";

export default class Demo_Label {
	public gRoot: FairyGUI.GComponent

	public frame: WindowFrame;
	public static URL: string = "ui://9leh0eyfw42o3j";

	public static createInstance(): Demo_Label {
		let inst = new Demo_Label();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Label"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Label {
		    let inst = new Demo_Label();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.frame = WindowFrame.fromInstance(this.gRoot.GetChildAt(1));
	}
}