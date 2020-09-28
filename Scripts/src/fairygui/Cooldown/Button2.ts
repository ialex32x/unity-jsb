/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Button2 {
	public gRoot: FairyGUI.GButton

	public btn: FairyGUI.GImage;
	public mask: FairyGUI.GImage;
	public static URL: string = "ui://y768eypfp3yap";

	public static createInstance(): Button2 {
		let inst = new Button2();
		inst.gRoot = <FairyGUI.GButton>(FairyGUI.UIPackage.CreateObject("Cooldown", "Button2"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Button2 {
		    let inst = new Button2();
		    inst.gRoot = <FairyGUI.GButton>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.btn = <FairyGUI.GImage>(this.gRoot.GetChildAt(3));
		this.mask = <FairyGUI.GImage>(this.gRoot.GetChildAt(5));
	}
}