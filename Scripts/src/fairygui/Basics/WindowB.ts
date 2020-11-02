/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
import WindowFrameB from "./WindowFrameB";

export default class WindowB {
	public gRoot: FairyGUI.GComponent

	public frame: WindowFrameB;
	public t1: FairyGUI.Transition;
	public static URL: string = "ui://9leh0eyf796x7a";

	public static createInstance(): WindowB {
		let inst = new WindowB();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "WindowB"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): WindowB {
		    let inst = new WindowB();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.frame = WindowFrameB.fromInstance(this.gRoot.GetChildAt(0));
		this.t1 = this.gRoot.GetTransitionAt(0);
	}
}