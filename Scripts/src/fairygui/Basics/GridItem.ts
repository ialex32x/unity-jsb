/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class GridItem {
	public gRoot: FairyGUI.GButton

	public t0: FairyGUI.GTextField;
	public t1: FairyGUI.GTextField;
	public t2: FairyGUI.GTextField;
	public star: FairyGUI.GProgressBar;
	public static URL: string = "ui://9leh0eyfa7vt7n";

	public static createInstance(): GridItem {
		let inst = new GridItem();
		inst.gRoot = <FairyGUI.GButton>(FairyGUI.UIPackage.CreateObject("Basics", "GridItem"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): GridItem {
		    let inst = new GridItem();
		    inst.gRoot = <FairyGUI.GButton>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.t0 = <FairyGUI.GTextField>(this.gRoot.GetChildAt(2));
		this.t1 = <FairyGUI.GTextField>(this.gRoot.GetChildAt(4));
		this.t2 = <FairyGUI.GTextField>(this.gRoot.GetChildAt(5));
		this.star = <FairyGUI.GProgressBar>(this.gRoot.GetChildAt(6));
	}
}