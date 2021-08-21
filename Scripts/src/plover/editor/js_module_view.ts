import { ModuleManager } from "jsb";
import { EditorGUI, EditorGUILayout, EditorStyles, EditorUtility } from "UnityEditor";
import { GUIContent, GUILayout, Vector2 } from "UnityEngine";
import { TEXT } from "../text/string_utils";
import { EditorWindowBase } from "./base/editor_window_base";
import { UTreeNode } from "./base/treenode";
import { reload } from "./js_reload";

export class JSModuleView extends EditorWindowBase {
    private _touch: any;

    Awake() {
        super.Awake();
        if (!this._touch) {
            this.updateModules();
        }
        this.toobarHeight = 26;
    }

    OnEnable() {
        this.titleContent = new GUIContent("JS Modules");
    }

    protected drawFolderView(data: any, node: UTreeNode) {
        if (!data) {
            return;
        }

        let mod: NodeModule = data;

        EditorGUILayout.Toggle(TEXT("Main"), mod == require.main);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(TEXT("Module ID"), mod.id);

        let doReload = false;
        if (mod["resolvername"] != "source") {
            EditorGUI.BeginDisabledGroup(true);
            doReload = GUILayout.Button(TEXT("Reload"));
            EditorGUI.EndDisabledGroup();
        } else {
            doReload = GUILayout.Button(TEXT("Reload"));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.TextField(TEXT("File Name"), mod.filename);
        if (typeof mod.parent === "object") {
            EditorGUILayout.TextField(TEXT("Parent"), mod.parent.id);
        } else {
            EditorGUILayout.TextField(TEXT("Parent"), TEXT("TOP LEVEL"));
        }

        if (doReload) {
            reload(mod);
        }
    }

    protected drawToolBar() {
        if (GUILayout.Button(this.TContent("Expand All", "Hierarchy", "Expand All"), EditorStyles.toolbarButton, GUILayout.Width(32), GUILayout.Height(32))) {
            this._treeView.expandAll();
        }

        if (GUILayout.Button(this.TContent("Collapse All", "Collapsed", "Collapse All"), EditorStyles.toolbarButton, GUILayout.Width(32), GUILayout.Height(32))) {
            this._treeView.collapseAll();
        }

        if (GUILayout.Button(this.TContent("Refresh", "Refresh", "Refresh"), EditorStyles.toolbarButton, GUILayout.Width(32), GUILayout.Height(32))) {
            this.updateModules();
        }
    }

    private updateModules() {
        this._treeView.removeAll();

        let cache = require.main["cache"];
        if (typeof cache === "undefined") {
            return;
        }

        this._touch = {};
        Object.keys(cache).forEach(name => {
            let mod = cache[name];
            this.addModule(mod, this._treeView.root);
        });
    }

    private getSimplifiedName(id: string) {
        let index = id.lastIndexOf('/');
        return index >= 0 ? id.substring(index + 1) : id;
    }

    private addModule(mod: NodeModule, treeNode: UTreeNode) {
        if (typeof this._touch[mod.id] !== "undefined") {
            // skip infinite loop
            return;
        }

        let childNode = treeNode.addFolderChild(this.getSimplifiedName(mod.id));

        this._touch[mod.id] = true;
        childNode.data = mod;
        childNode.isEditable = false;
        if (typeof mod.children !== "undefined") {
            for (let i = 0; i < mod.children.length; i++) {
                let child = mod.children[i];

                this.addModule(child, childNode);
            }
        }
    }
}

