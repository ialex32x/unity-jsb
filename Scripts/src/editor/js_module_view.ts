import { ModuleManager } from "jsb";
import { EditorGUI, EditorGUILayout, EditorWindow } from "UnityEditor";
import { GUIContent, GUILayout, Vector2 } from "UnityEngine";
import { reload } from "./js_reload";

export class JSModuleView extends EditorWindow {
    private _touch: any;
    private _sv = Vector2.zero;

    OnEnable() {
        this.titleContent = new GUIContent("JS Modules");
    }

    private drawModule(mod: NodeModule) {
        GUILayout.Space(12);

        if (typeof this._touch[mod.id] !== "undefined") {
            EditorGUILayout.TextField("Module ID", mod.id);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Module ID", mod.id);
        let doReload = false;
        if (mod["resolvername"] != "source") {
            EditorGUI.BeginDisabledGroup(true);
            doReload = GUILayout.Button("Reload");
            EditorGUI.EndDisabledGroup();
        } else {
            doReload = GUILayout.Button("Reload");
        }
        EditorGUILayout.EndHorizontal();

        this._touch[mod.id] = true;
        EditorGUILayout.TextField("File Name", mod.filename);
        if (typeof mod.parent === "object") {
            EditorGUILayout.TextField("Parent", mod.parent.id);
        } else {
            EditorGUILayout.TextField("Parent", "TOP LEVEL");
        }

        if (typeof mod.children !== "undefined") {
            EditorGUILayout.IntField("Children", mod.children.length);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(50);
            EditorGUILayout.BeginVertical();
            for (let i = 0; i < mod.children.length; i++) {
                let child = mod.children[i];


                EditorGUILayout.TextField("Child", child.id);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        if (doReload) {
            reload(mod);
        }
    }

    OnGUI() {
        let cache = require.main["cache"];
        if (typeof cache === "undefined") {
            return;
        }

        this._touch = {};
        this._sv = EditorGUILayout.BeginScrollView(this._sv)
        Object.keys(cache).forEach(name => {
            let mod = cache[name];
            this.drawModule(mod);
        });
        EditorGUILayout.EndScrollView();
    }
}

