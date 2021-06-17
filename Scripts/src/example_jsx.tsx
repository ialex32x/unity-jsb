import { Object, GameObject, Resources, RectTransform, Vector2 } from "UnityEngine";
import { ScriptType } from "./plover/editor/editor_decorators";
import { JSXWidgetBridge } from "./plover/jsx/bridge";
import * as JSX from "./plover/jsx/element";
import { ViewModel } from "./plover/jsx/vue";

export interface TestData {
    name: string,
    tick: number;
}

@ScriptType()
export class MyWidgetTest extends JSXWidgetBridge {
    private _data: TestData;
    private _timer: any;

    get data() { return this._data; }

    Awake() {
        this._data = ViewModel.create({ name: "Unity", tick: 0 });
        this._widget =
            <widget class={this}>
                <text name="label" text="Hello {{this.name}} {{this.tick}}" />
            </widget>

        this._timer = setInterval(() => {
            this._data.tick++;
        }, 1000);
        this._data.tick++;
    }

    OnDestroy() {
        super.OnDestroy();
        clearInterval(this._timer);
    }
}

let parent = GameObject.Find("/Canvas").transform;
let go = Object.Instantiate(Resources.Load("prefab/jsx_test_ui")) as GameObject;

go.transform.SetParent(parent);
let rect = go.GetComponent(RectTransform);

rect.anchorMin = Vector2.zero;
rect.anchorMax = Vector2.one;
rect.anchoredPosition = Vector2.zero;
console.log("load", go, go.AddComponent(MyWidgetTest));
