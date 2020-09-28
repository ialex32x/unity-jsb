/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Button1 {
	public gRoot: FairyGUI.GButton

	public mask: FairyGUI.GImage;
	public static URL: string = "ui://y768eypfltiql";

	public static createInstance(): Button1 {
		let inst = new Button1();
		inst.gRoot = <FairyGUI.GButton>(FairyGUI.UIPackage.CreateObject("Cooldown", "Button1"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Button1 {
		    let inst = new Button1();
		    inst.gRoot = <FairyGUI.GButton>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.mask = <FairyGUI.GImage>(this.gRoot.GetChildAt(3));
	}
}