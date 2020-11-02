/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class WindowFrame {
	public gRoot: FairyGUI.GLabel

	public closeButton: FairyGUI.GButton;
	public dragArea: FairyGUI.GGraph;
	public contentArea: FairyGUI.GGraph;
	public static URL: string = "ui://9leh0eyfrt103l";

	public static createInstance(): WindowFrame {
		let inst = new WindowFrame();
		inst.gRoot = <FairyGUI.GLabel>(FairyGUI.UIPackage.CreateObject("Basics", "WindowFrame"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): WindowFrame {
		    let inst = new WindowFrame();
		    inst.gRoot = <FairyGUI.GLabel>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.closeButton = <FairyGUI.GButton>(this.gRoot.GetChildAt(1));
		this.dragArea = <FairyGUI.GGraph>(this.gRoot.GetChildAt(2));
		this.contentArea = <FairyGUI.GGraph>(this.gRoot.GetChildAt(4));
	}
}