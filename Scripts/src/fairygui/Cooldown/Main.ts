/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

import Button1 from "./Button1";
import Button2 from "./Button2";

export default class Main {
	public gRoot: FairyGUI.GComponent

	public b0: Button1;
	public b1: Button2;
	public static URL: string = "ui://y768eypffvaib";

	public static createInstance(): Main {
		let inst = new Main();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Cooldown", "Main"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Main {
		    let inst = new Main();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.b0 = Button1.fromInstance(this.gRoot.GetChildAt(0));
		this.b1 = Button2.fromInstance(this.gRoot.GetChildAt(1));
	}
}