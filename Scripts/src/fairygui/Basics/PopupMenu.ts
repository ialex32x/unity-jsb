/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class PopupMenu {
	public gRoot: FairyGUI.GComponent

	public list: FairyGUI.GList;
	public static URL: string = "ui://9leh0eyfl6f46x";

	public static createInstance(): PopupMenu {
		let inst = new PopupMenu();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "PopupMenu"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): PopupMenu {
		    let inst = new PopupMenu();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.list = <FairyGUI.GList>(this.gRoot.GetChildAt(1));
	}
}