"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.Something5Behaviour = exports.RotateBehaviour = exports.MySubClass = exports.MyClass = exports.ASimpleGuiDialog = void 0;
const UnityEngine_1 = require("UnityEngine");
const jsb = require("jsb");
const UnityEngine_UI_1 = require("UnityEngine.UI");
const class_decorators_1 = require("plover/runtime/class_decorators");
let ASimpleGuiDialog = class ASimpleGuiDialog extends UnityEngine_1.MonoBehaviour {
    //TODO: [NOT_IMPLEMENTED] 使 UnityEvent 识别并接受脚本函数作为回调
    onClicked() {
        console.log("you clicked the button");
    }
};
__decorate([
    class_decorators_1.ScriptProperty({ type: "object", bind: { widget: UnityEngine_UI_1.Button } })
], ASimpleGuiDialog.prototype, "nameLabel", void 0);
__decorate([
    class_decorators_1.ScriptFunction()
], ASimpleGuiDialog.prototype, "onClicked", null);
ASimpleGuiDialog = __decorate([
    class_decorators_1.ScriptType()
], ASimpleGuiDialog);
exports.ASimpleGuiDialog = ASimpleGuiDialog;
//TODO constructor is not called when using v8-bridge as backend
let MyClass = class MyClass extends UnityEngine_1.MonoBehaviour {
    constructor() {
        super(...arguments);
        this.vv = 0;
        this._tick = 0;
    }
    Awake() {
        this.vv = 0;
        this._tick = 0;
        console.log("MyClass.Awake", this._tick++);
    }
    async OnEnable() {
        console.log("MyClass.OnEnable", this._tick++);
        await jsb.Yield(new UnityEngine_1.WaitForSeconds(1));
        console.log("MyClass.OnEnable (delayed)", this._tick++);
    }
    OnDisable() {
        console.log("MyClass.OnDisable", this._tick++);
    }
    OnDestroy() {
        console.log("MyClass.OnDestroy", this._tick++);
    }
    OnApplicationFocus(hasFocus) {
        console.log("OnApplicationFocus:", hasFocus);
    }
    OnApplicationPause(pause) {
        console.log("OnApplicationPause:", pause);
    }
    speak(text) {
        console.log("modified", text);
    }
    async test() {
        console.log("MyClass.test (will be destroied after 5 secs.", this.transform);
        await jsb.Yield(new UnityEngine_1.WaitForSeconds(5));
        UnityEngine_1.Object.Destroy(this.gameObject);
    }
};
__decorate([
    class_decorators_1.ScriptNumber()
], MyClass.prototype, "vv", void 0);
__decorate([
    class_decorators_1.ScriptInteger({ editable: false, serializable: false })
], MyClass.prototype, "_tick", void 0);
MyClass = __decorate([
    class_decorators_1.ScriptType()
], MyClass);
exports.MyClass = MyClass;
let MySubClass = class MySubClass extends MyClass {
    Awake() {
        super.Awake();
        console.log("MySubClass.Awake", this._tick++);
    }
    Update() {
        if (UnityEngine_1.Input.GetMouseButtonUp(0)) {
            let ray = UnityEngine_1.Camera.main.ScreenPointToRay(UnityEngine_1.Input.mousePosition);
            let hitInfo = { type: UnityEngine_1.RaycastHit };
            let layerMask = 1 << UnityEngine_1.LayerMask.NameToLayer("Default");
            console.log("hittest", layerMask);
            if (UnityEngine_1.Physics.Raycast(ray, hitInfo, 1000, layerMask)) {
                console.log("hit", hitInfo.value.transform.name);
            }
        }
    }
    play() {
        console.log("MySubClass.play");
    }
};
MySubClass = __decorate([
    class_decorators_1.ScriptType()
], MySubClass);
exports.MySubClass = MySubClass;
let RotateBehaviour = class RotateBehaviour extends UnityEngine_1.MonoBehaviour {
    constructor() {
        super(...arguments);
        this._rotation = 0;
        this.rotationSpeed = 50;
    }
    Reset() {
        this._rotation = 0;
    }
    Awake() {
        this._rotation = 0;
        this.rotationSpeed = 50;
        let ps = this.GetComponent(UnityEngine_1.ParticleSystem);
        if (ps) {
            ps.main.simulationSpace = UnityEngine_1.ParticleSystemSimulationSpace.World;
            console.log("ps.main.simulationSpace:", ps.main.simulationSpace);
        }
    }
    Update() {
        this._rotation += this.rotationSpeed * UnityEngine_1.Time.deltaTime;
        if (jsb.isOperatorOverloadingSupported) {
            //@ts-ignore
            let p = UnityEngine_1.Quaternion.Euler(0, this._rotation, 0) * UnityEngine_1.Vector3.right * 5;
            p.z *= 0.5;
            this.transform.localPosition = p;
        }
        else {
            let p = UnityEngine_1.Vector3.op_Multiply(UnityEngine_1.Quaternion.op_Multiply(UnityEngine_1.Quaternion.Euler(0, this._rotation, 0), UnityEngine_1.Vector3.right), 5);
            p.z *= 0.5;
            this.transform.localPosition = p;
        }
    }
};
__decorate([
    class_decorators_1.ScriptNumber({ serializable: false })
], RotateBehaviour.prototype, "rotationSpeed", void 0);
RotateBehaviour = __decorate([
    class_decorators_1.ScriptType()
], RotateBehaviour);
exports.RotateBehaviour = RotateBehaviour;
let Something5Behaviour = class Something5Behaviour extends UnityEngine_1.MonoBehaviour {
};
__decorate([
    class_decorators_1.ScriptProperty({ type: "Vector3" })
], Something5Behaviour.prototype, "pos", void 0);
Something5Behaviour = __decorate([
    class_decorators_1.ScriptType()
], Something5Behaviour);
exports.Something5Behaviour = Something5Behaviour;
//# sourceMappingURL=sample_monobehaviour.js.map