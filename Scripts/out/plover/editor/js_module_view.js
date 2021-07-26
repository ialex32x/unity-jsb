"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.JSModuleView = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const editor_window_base_1 = require("./base/editor_window_base");
const js_reload_1 = require("./js_reload");
class JSModuleView extends editor_window_base_1.EditorWindowBase {
    Awake() {
        super.Awake();
        if (!this._touch) {
            this.updateModules();
        }
        this.toobarHeight = 26;
    }
    OnEnable() {
        this.titleContent = new UnityEngine_1.GUIContent("JS Modules");
    }
    drawFolderView(data, node) {
        if (!data) {
            return;
        }
        let mod = data;
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
        UnityEditor_1.EditorGUILayout.TextField("File Name", mod.filename);
        if (typeof mod.parent === "object") {
            UnityEditor_1.EditorGUILayout.TextField("Parent", mod.parent.id);
        }
        else {
            UnityEditor_1.EditorGUILayout.TextField("Parent", "TOP LEVEL");
        }
        if (doReload) {
            js_reload_1.reload(mod);
        }
    }
    drawToolBar() {
        if (UnityEngine_1.GUILayout.Button(this.TContent("Expand All", "Hierarchy", "Expand All"), UnityEditor_1.EditorStyles.toolbarButton, UnityEngine_1.GUILayout.Width(32), UnityEngine_1.GUILayout.Height(32))) {
            this._treeView.expandAll();
        }
        if (UnityEngine_1.GUILayout.Button(this.TContent("Collapse All", "Collapsed", "Collapse All"), UnityEditor_1.EditorStyles.toolbarButton, UnityEngine_1.GUILayout.Width(32), UnityEngine_1.GUILayout.Height(32))) {
            this._treeView.collapseAll();
        }
        if (UnityEngine_1.GUILayout.Button(this.TContent("Refresh", "Refresh", "Refresh"), UnityEditor_1.EditorStyles.toolbarButton, UnityEngine_1.GUILayout.Width(32), UnityEngine_1.GUILayout.Height(32))) {
            this.updateModules();
        }
    }
    updateModules() {
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
    getSimplifiedName(id) {
        let index = id.lastIndexOf('/');
        return index >= 0 ? id.substring(index + 1) : id;
    }
    addModule(mod, treeNode) {
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
exports.JSModuleView = JSModuleView;
//# sourceMappingURL=js_module_view.js.map