/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Demo_Graph {
	public gRoot: FairyGUI.GComponent

	public polygon: FairyGUI.GGraph;
	public polygon2: FairyGUI.GGraph;
	public line: FairyGUI.GGraph;
	public line3: FairyGUI.GImage;
	public pie: FairyGUI.GGraph;
	public radial: FairyGUI.GGraph;
	public trapezoid: FairyGUI.GGraph;
	public line2: FairyGUI.GGraph;
	public static URL: string = "ui://9leh0eyfhixt1m";

	public static createInstance(): Demo_Graph {
		let inst = new Demo_Graph();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Demo_Graph"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Demo_Graph {
		    let inst = new Demo_Graph();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.polygon = <FairyGUI.GGraph>(this.gRoot.GetChildAt(5));
		this.polygon2 = <FairyGUI.GGraph>(this.gRoot.GetChildAt(6));
		this.line = <FairyGUI.GGraph>(this.gRoot.GetChildAt(7));
		this.line3 = <FairyGUI.GImage>(this.gRoot.GetChildAt(8));
		this.pie = <FairyGUI.GGraph>(this.gRoot.GetChildAt(12));
		this.radial = <FairyGUI.GGraph>(this.gRoot.GetChildAt(14));
		this.trapezoid = <FairyGUI.GGraph>(this.gRoot.GetChildAt(15));
		this.line2 = <FairyGUI.GGraph>(this.gRoot.GetChildAt(16));
	}
}