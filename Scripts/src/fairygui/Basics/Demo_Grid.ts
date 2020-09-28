/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Demo_Grid {
	public gRoot: FairyGUI.GComponent

	public list1: FairyGUI.GList;
	public list2: FairyGUI.GList;
	public static URL: string = "ui://9leh0eyfc2z47l";

	public static createInstance(): Demo_Grid {
		let inst = new Demo_Grid();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Grid"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Grid {
		    let inst = new Demo_Grid();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.list1 = <FairyGUI.GList>(this.gRoot.GetChildAt(2));
		this.list2 = <FairyGUI.GList>(this.gRoot.GetChildAt(10));
	}
}