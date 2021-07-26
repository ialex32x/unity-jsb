"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.HSplitView = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
class HSplitView {
    constructor() {
        this.init = false; // 是否初始化
        this.splitPivot = 0;
        this.resize = false;
        this.cursorChangeRect = UnityEngine_1.Rect.zero;
        this.cursorHintRect = UnityEngine_1.Rect.zero;
        this.cursorHintSize = 2;
        this.cursorSize = 6;
        this.cursorHintColor = new UnityEngine_1.Color(0, 0, 0, 0.25);
    }
    draw(window, startY, fullWidth, fullHeight) {
        if (!this.init) {
            this.init = true;
            this.splitPivot = Math.min(Math.max(fullWidth * .25, 10), fullWidth - 10);
            this.cursorChangeRect.Set(this.splitPivot - 2, startY, this.cursorSize, fullHeight);
            this.cursorHintRect.Set(this.splitPivot - 2, startY, this.cursorHintSize, fullHeight);
        }
        else {
            this.cursorChangeRect.height = fullHeight;
            this.cursorHintRect.height = fullHeight;
        }
        UnityEditor_1.EditorGUI.DrawRect(this.cursorHintRect, this.cursorHintColor);
        UnityEditor_1.EditorGUIUtility.AddCursorRect(this.cursorChangeRect, UnityEditor_1.MouseCursor.ResizeHorizontal);
        if (UnityEngine_1.Event.current.type == UnityEngine_1.EventType.MouseDown && this.cursorChangeRect.Contains(UnityEngine_1.Event.current.mousePosition)) {
            this.resize = true;
        }
        if (this.resize) {
            let y = this.cursorChangeRect.y;
            let h = this.cursorChangeRect.height;
            this.splitPivot = Math.min(Math.max(UnityEngine_1.Event.current.mousePosition.x, 10), fullWidth - 10);
            this.cursorChangeRect.Set(this.splitPivot - 2, y, this.cursorSize, h);
            this.cursorHintRect.Set(this.splitPivot - 2, y, this.cursorHintSize, h);
            window.Repaint();
        }
        if (UnityEngine_1.Event.current.type == UnityEngine_1.EventType.MouseUp) {
            this.resize = false;
        }
    }
}
exports.HSplitView = HSplitView;
//# sourceMappingURL=splitview.js.map