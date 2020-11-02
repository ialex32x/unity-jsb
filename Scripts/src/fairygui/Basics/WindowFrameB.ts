/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class WindowFrameB {
	public gRoot: FairyGUI.GLabel

	public dragArea: FairyGUI.GGraph;
	public closeButton: FairyGUI.GButton;
	public static URL: string = "ui://9leh0eyfniii7d";

	public static createInstance(): WindowFrameB {
		let inst = new WindowFrameB();
		inst.gRoot = <FairyGUI.GLabel>(FairyGUI.UIPackage.CreateObject("Basics", "WindowFrameB"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): WindowFrameB {
		    let inst = new WindowFrameB();
		    inst.gRoot = <FairyGUI.GLabel>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.dragArea = <FairyGUI.GGraph>(this.gRoot.GetChildAt(1));
		this.closeButton = <FairyGUI.GButton>(this.gRoot.GetChildAt(2));
	}
}