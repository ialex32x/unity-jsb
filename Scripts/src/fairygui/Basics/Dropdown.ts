/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Dropdown {
	public gRoot: FairyGUI.GComboBox

	public button: FairyGUI.GButton;
	public static URL: string = "ui://9leh0eyfzd9g41";

	public static createInstance(): Dropdown {
		let inst = new Dropdown();
		inst.gRoot = <FairyGUI.GComboBox>(FairyGUI.UIPackage.CreateObject("Basics", "Dropdown"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Dropdown {
		    let inst = new Dropdown();
		    inst.gRoot = <FairyGUI.GComboBox>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.button = <FairyGUI.GButton>(this.gRoot.GetChildAt(0));
	}
}