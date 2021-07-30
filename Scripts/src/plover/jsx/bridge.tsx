import { MonoBehaviour } from "UnityEngine";
import { ScriptType } from "../runtime/class_decorators";
import * as JSX from "./element";

@ScriptType()
export abstract class JSXWidgetBridge extends MonoBehaviour {
    protected _widget: JSX.JSXWidget;

    get data() { return null; }

    OnDestroy() {
        if (this._widget) {
            this._widget.destroy();
        }
    }
}
