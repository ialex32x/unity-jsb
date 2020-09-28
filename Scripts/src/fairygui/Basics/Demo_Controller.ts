/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Demo_Controller {
	public gRoot: FairyGUI.GComponent

	public c1: FairyGUI.Controller;
	public c2: FairyGUI.Controller;
	public switchBtn: FairyGUI.GButton;
	public static URL: string = "ui://9leh0eyfwa8u2v";

	public static createInstance(): Demo_Controller {
		let inst = new Demo_Controller();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Controller"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Controller {
		    let inst = new Demo_Controller();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.c1 = this.gRoot.GetControllerAt(0);
		this.c2 = this.gRoot.GetControllerAt(1);
		this.switchBtn = <FairyGUI.GButton>(this.gRoot.GetChildAt(13));
	}
}