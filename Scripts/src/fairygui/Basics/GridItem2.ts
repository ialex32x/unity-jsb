/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class GridItem2 {
	public gRoot: FairyGUI.GButton

	public t3: FairyGUI.GTextField;
	public t1: FairyGUI.GTextField;
	public cb: FairyGUI.GButton;
	public mc: FairyGUI.GMovieClip;
	public static URL: string = "ui://9leh0eyfatih7o";

	public static createInstance(): GridItem2 {
		let inst = new GridItem2();
		inst.gRoot = <FairyGUI.GButton>(FairyGUI.UIPackage.CreateObject("Basics", "GridItem2"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): GridItem2 {
		    let inst = new GridItem2();
		    inst.gRoot = <FairyGUI.GButton>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.t3 = <FairyGUI.GTextField>(this.gRoot.GetChildAt(2));
		this.t1 = <FairyGUI.GTextField>(this.gRoot.GetChildAt(4));
		this.cb = <FairyGUI.GButton>(this.gRoot.GetChildAt(5));
		this.mc = <FairyGUI.GMovieClip>(this.gRoot.GetChildAt(6));
	}
}