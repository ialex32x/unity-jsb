/** This is an automatically generated class by FairyGUI. Please do not modify it. **/
import * as FairyGUI from "FairyGUI";
export default class Main {
	public gRoot: FairyGUI.GComponent

	public c1: FairyGUI.Controller;
	public container: FairyGUI.GComponent;
	public btn_Back: FairyGUI.GButton;
	public btn_Button: FairyGUI.GButton;
	public btn_Image: FairyGUI.GButton;
	public btn_Graph: FairyGUI.GButton;
	public btn_MovieClip: FairyGUI.GButton;
	public btn_Depth: FairyGUI.GButton;
	public btn_Loader: FairyGUI.GButton;
	public btn_List: FairyGUI.GButton;
	public btn_ProgressBar: FairyGUI.GButton;
	public btn_Slider: FairyGUI.GButton;
	public btn_ComboBox: FairyGUI.GButton;
	public btn_ClipAndScroll: FairyGUI.GButton;
	public btn_Controller: FairyGUI.GButton;
	public btn_Relation: FairyGUI.GButton;
	public btn_Label: FairyGUI.GButton;
	public btn_Popup: FairyGUI.GButton;
	public btn_Window: FairyGUI.GButton;
	public btn_DragAndDrop: FairyGUI.GButton;
	public btn_Component: FairyGUI.GButton;
	public btn_Grid: FairyGUI.GButton;
	public btn_Text: FairyGUI.GButton;
	public btns: FairyGUI.GGroup;
	public static URL: string = "ui://9leh0eyfrpmb1c";

	public static createInstance(): Main {
		let inst = new Main();
		inst.gRoot = <FairyGUI.GComponent>(FairyGUI.UIPackage.CreateObject("Basics", "Main"));
		inst.onConstruct();
		return inst;
	}

	public static fromInstance(gRoot: FairyGUI.GObject): Main {
		    let inst = new Main();
		    inst.gRoot = <FairyGUI.GComponent>gRoot;
		    inst.onConstruct();
		    return inst;
	}

	protected onConstruct(): void {
		this.c1 = this.gRoot.GetControllerAt(0);
		this.container = <FairyGUI.GComponent>(this.gRoot.GetChildAt(1));
		this.btn_Back = <FairyGUI.GButton>(this.gRoot.GetChildAt(3));
		this.btn_Button = <FairyGUI.GButton>(this.gRoot.GetChildAt(4));
		this.btn_Image = <FairyGUI.GButton>(this.gRoot.GetChildAt(5));
		this.btn_Graph = <FairyGUI.GButton>(this.gRoot.GetChildAt(6));
		this.btn_MovieClip = <FairyGUI.GButton>(this.gRoot.GetChildAt(7));
		this.btn_Depth = <FairyGUI.GButton>(this.gRoot.GetChildAt(8));
		this.btn_Loader = <FairyGUI.GButton>(this.gRoot.GetChildAt(9));
		this.btn_List = <FairyGUI.GButton>(this.gRoot.GetChildAt(10));
		this.btn_ProgressBar = <FairyGUI.GButton>(this.gRoot.GetChildAt(11));
		this.btn_Slider = <FairyGUI.GButton>(this.gRoot.GetChildAt(12));
		this.btn_ComboBox = <FairyGUI.GButton>(this.gRoot.GetChildAt(13));
		this.btn_ClipAndScroll = <FairyGUI.GButton>(this.gRoot.GetChildAt(14));
		this.btn_Controller = <FairyGUI.GButton>(this.gRoot.GetChildAt(15));
		this.btn_Relation = <FairyGUI.GButton>(this.gRoot.GetChildAt(16));
		this.btn_Label = <FairyGUI.GButton>(this.gRoot.GetChildAt(17));
		this.btn_Popup = <FairyGUI.GButton>(this.gRoot.GetChildAt(18));
		this.btn_Window = <FairyGUI.GButton>(this.gRoot.GetChildAt(19));
		this.btn_DragAndDrop = <FairyGUI.GButton>(this.gRoot.GetChildAt(20));
		this.btn_Component = <FairyGUI.GButton>(this.gRoot.GetChildAt(21));
		this.btn_Grid = <FairyGUI.GButton>(this.gRoot.GetChildAt(22));
		this.btn_Text = <FairyGUI.GButton>(this.gRoot.GetChildAt(23));
		this.btns = <FairyGUI.GGroup>(this.gRoot.GetChildAt(24));
	}
}