import { EditorGUI, EditorGUIUtility, EditorWindow, MouseCursor } from "UnityEditor";
import { Color, Event, EventType, GUI, Rect } from "UnityEngine";

export class HSplitView {
    init: boolean = false; // 是否初始化
    splitPivot: number = 0;
    resize: boolean = false;
    cursorChangeRect = Rect.zero;
    cursorHintRect = Rect.zero;

    cursorHintSize = 2;
    cursorSize = 6;
    cursorHintColor = new Color(0, 0, 0, 0.25);

    draw(window: EditorWindow, startY: number, fullWidth: number, fullHeight: number) {
        if (!this.init) {
            this.init = true;
            this.splitPivot = Math.min(Math.max(fullWidth * .25, 10), fullWidth - 10);
            this.cursorChangeRect.Set(this.splitPivot - 2, startY, this.cursorSize, fullHeight);
            this.cursorHintRect.Set(this.splitPivot - 2, startY, this.cursorHintSize, fullHeight);
        } else {
            this.cursorChangeRect.height = fullHeight;
            this.cursorHintRect.height = fullHeight;
        }

        EditorGUI.DrawRect(this.cursorHintRect, this.cursorHintColor);
        EditorGUIUtility.AddCursorRect(this.cursorChangeRect, MouseCursor.ResizeHorizontal);

        if (Event.current.type == EventType.MouseDown && this.cursorChangeRect.Contains(Event.current.mousePosition)) {
            this.resize = true;
        }

        if (this.resize) {
            let y = this.cursorChangeRect.y;
            let h = this.cursorChangeRect.height;
            this.splitPivot = Math.min(Math.max(Event.current.mousePosition.x, 10), fullWidth - 10);
            this.cursorChangeRect.Set(this.splitPivot - 2, y, this.cursorSize, h);
            this.cursorHintRect.Set(this.splitPivot - 2, y, this.cursorHintSize, h);
            window.Repaint();
        }

        if (Event.current.type == EventType.MouseUp) {
            this.resize = false;
        }
    }
}