/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Demo_Relation {
	public gRoot: FairyGUI.GComponent

	public c1: FairyGUI.Controller;
	public static URL: string = "ui://9leh0eyfwa8u2x";

	public static createInstance(): Demo_Relation {
		let inst = new Demo_Relation();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Relation"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Relation {
		    let inst = new Demo_Relation();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.c1 = this.gRoot.GetControllerAt(0);
	}
}