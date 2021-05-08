"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.JSModuleView = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const js_reload_1 = require("./js_reload");
class JSModuleView extends UnityEditor_1.EditorWindow {
    constructor() {
        super(...arguments);
        this._sv = UnityEngine_1.Vector2.zero;
    }
    OnEnable() {
        this.titleContent = new UnityEngine_1.GUIContent("JS Modules");
    }
    drawModule(mod) {
        UnityEngine_1.GUILayout.Space(12);
        if (typeof this._touch[mod.id] !== "undefined") {
            UnityEditor_1.EditorGUILayout.TextField("Module ID", mod.id);
            return;
        }
        UnityEditor_1.EditorGUILayout.BeginHorizontal();
        UnityEditor_1.EditorGUILayout.TextField("Module ID", mod.id);
        let doReload = false;
        if (mod["resolvername"] != "source") {
            UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
            doReload = UnityEngine_1.GUILayout.Button("Reload");
            UnityEditor_1.EditorGUI.EndDisabledGroup();
        }
        else {
            doReload = UnityEngine_1.GUILayout.Button("Reload");
        }
        UnityEditor_1.EditorGUILayout.EndHorizontal();
        this._touch[mod.id] = true;
        UnityEditor_1.EditorGUILayout.TextField("File Name", mod.filename);
        if (typeof mod.parent === "object") {
            UnityEditor_1.EditorGUILayout.TextField("Parent", mod.parent.id);
        }
        else {
            UnityEditor_1.EditorGUILayout.TextField("Parent", "TOP LEVEL");
        }
        if (typeof mod.children !== "undefined") {
            UnityEditor_1.EditorGUILayout.IntField("Children", mod.children.length);
            UnityEditor_1.EditorGUILayout.BeginHorizontal();
            UnityEngine_1.GUILayout.Space(50);
            UnityEditor_1.EditorGUILayout.BeginVertical();
            for (let i = 0; i < mod.children.length; i++) {
                let child = mod.children[i];
                UnityEditor_1.EditorGUILayout.TextField("Child", child.id);
            }
            UnityEditor_1.EditorGUILayout.EndVertical();
            UnityEditor_1.EditorGUILayout.EndHorizontal();
        }
        if (doReload) {
            js_reload_1.reload(mod);
        }
    }
    OnGUI() {
        let cache = require.main["cache"];
        if (typeof cache === "undefined") {
            return;
        }
        this._touch = {};
        this._sv = UnityEditor_1.EditorGUILayout.BeginScrollView(this._sv);
        Object.keys(cache).forEach(name => {
            let mod = cache[name];
            this.drawModule(mod);
        });
        UnityEditor_1.EditorGUILayout.EndScrollView();
    }
}
exports.JSModuleView = JSModuleView;
//# sourceMappingURL=js_module_view.js.map