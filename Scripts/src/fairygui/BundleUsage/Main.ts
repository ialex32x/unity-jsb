/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

export default class Main {
	public gRoot: FairyGUI.GComponent

	public theLabel: FairyGUI.GTextField;
	public t0: FairyGUI.Transition;
	public static URL: string = "ui://d8m5tmokfou90";

	public static createInstance(): Main {
		let inst = new Main();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("BundleUsage", "Main"));
		inst.onConstruct();
		return inst;
	}

	protected onConstruct(): void {
		this.theLabel = <FairyGUI.GTextField>(this.gRoot.GetChildAt(1));
		this.t0 = this.gRoot.GetTransitionAt(0);
	}
}