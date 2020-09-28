/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class ComboBoxPopup {
	public gRoot: FairyGUI.GComponent

	public list: FairyGUI.GList;
	public static URL: string = "ui://9leh0eyfrt103y";

	public static createInstance(): ComboBoxPopup {
		let inst = new ComboBoxPopup();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "ComboBoxPopup"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): ComboBoxPopup {
		    let inst = new ComboBoxPopup();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.list = <FairyGUI.GList>(this.gRoot.GetChildAt(1));
	}
}