/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

import WindowFrame from "./WindowFrame";

export default class WindowA {
	public gRoot: FairyGUI.GComponent

	public frame: WindowFrame;
	public static URL: string = "ui://9leh0eyfl6f473";

	public static createInstance(): WindowA {
		let inst = new WindowA();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "WindowA"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): WindowA {
		    let inst = new WindowA();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.frame = WindowFrame.fromInstance(this.gRoot.GetChildAt(0));
	}
}