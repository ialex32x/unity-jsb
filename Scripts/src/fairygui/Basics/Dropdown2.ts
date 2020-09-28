/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Dropdown2 {
	public gRoot: FairyGUI.GComponent

	public list: FairyGUI.GList;
	public static URL: string = "ui://9leh0eyfzd9g47";

	public static createInstance(): Dropdown2 {
		let inst = new Dropdown2();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Dropdown2"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Dropdown2 {
		    let inst = new Dropdown2();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.list = <FairyGUI.GList>(this.gRoot.GetChildAt(1));
	}
}