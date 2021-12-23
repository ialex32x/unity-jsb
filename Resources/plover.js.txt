var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
define("plover/events/dispatcher", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.EventDispatcher = exports.Dispatcher = exports.Handler = void 0;
    class Handler {
        constructor(caller, fn, once) {
            this.caller = caller;
            this.fn = fn;
            this.once = !!once;
        }
        invoke(arg0, arg1, arg2) {
            if (this.fn) {
                this.fn.call(this.caller, arg0, arg1, arg2);
            }
        }
    }
    exports.Handler = Handler;
    /**
     * 简单的事件分发器实现
     * 此实现功能与 DuktapeJS.Dispatcher 基本一致,
     * 但 DuktapeJS.Dispatcher 不保证事件响应顺序, 但效率更高 (因为复用了中途移除的索引)
     */
    class Dispatcher {
        constructor() {
            this._handlers = [];
        }
        on(caller, fn) {
            let handler = new Handler(caller, fn);
            this._handlers.push(handler);
            return handler;
        }
        once(caller, fn) {
            let handler = new Handler(caller, fn, true);
            this._handlers.push(handler);
            return handler;
        }
        off(caller, fn) {
            let size = this._handlers.length;
            if (typeof fn === "undefined") {
                let found = false;
                for (let i = 0; i < size;) {
                    let item = this._handlers[i];
                    if (item.caller == caller) {
                        found = true;
                        item.fn = null;
                        item.caller = null;
                        this._handlers.splice(i, 1);
                        size--;
                    }
                    else {
                        i++;
                    }
                }
                return found;
            }
            for (let i = 0; i < size; i++) {
                let item = this._handlers[i];
                if (item.caller == caller && item.fn == fn) {
                    item.fn = null;
                    item.caller = null;
                    this._handlers.splice(i, 1);
                    return true;
                }
            }
            return false;
        }
        /**
         * 移除所有处理器
         */
        clear() {
            this._handlers.splice(0);
        }
        dispatch(arg0, arg1, arg2) {
            let size = this._handlers.length;
            if (size == 0) {
                return;
            }
            if (size == 1) {
                let item = this._handlers[0];
                if (item.once) {
                    this._handlers.splice(0, 1);
                }
                item.invoke(arg0, arg1, arg2);
                return;
            }
            if (size == 2) {
                let item0 = this._handlers[0];
                let item1 = this._handlers[1];
                if (item0.once) {
                    if (item1.once) {
                        this._handlers.splice(0, 2);
                    }
                    else {
                        this._handlers.splice(0, 1);
                    }
                }
                else {
                    if (item1.once) {
                        this._handlers.splice(1, 1);
                    }
                }
                item0.invoke(arg0, arg1, arg2);
                item1.invoke(arg0, arg1, arg2);
                return;
            }
            let copy = new Array(...this._handlers);
            for (let i = 0; i < size; i++) {
                let item = copy[i];
                if (item.once) {
                    let found = this._handlers.indexOf(item);
                    if (found >= 0) {
                        this._handlers.splice(found, 1);
                    }
                }
                copy[i].invoke(arg0, arg1, arg2);
            }
        }
    }
    exports.Dispatcher = Dispatcher;
    /**
     * 按事件名派发
     */
    class EventDispatcher {
        constructor() {
            this._dispatcher = {};
        }
        on(evt, caller, fn) {
            let dispatcher = this._dispatcher[evt];
            if (typeof dispatcher === "undefined") {
                dispatcher = this._dispatcher[evt] = new Dispatcher();
            }
            dispatcher.on(caller, fn);
        }
        once(evt, caller, fn) {
            let dispatcher = this._dispatcher[evt];
            if (typeof dispatcher === "undefined") {
                dispatcher = this._dispatcher[evt] = new Dispatcher();
            }
            dispatcher.once(caller, fn);
        }
        off(evt, caller, fn) {
            let dispatcher = this._dispatcher[evt];
            if (typeof dispatcher !== "undefined") {
                dispatcher.off(caller, fn);
            }
        }
        clear() {
            for (let evt in this._dispatcher) {
                let dispatcher = this._dispatcher[evt];
                if (dispatcher instanceof Dispatcher) {
                    dispatcher.clear();
                }
            }
        }
        /**
         * 派发指定事件
         */
        dispatch(evt, arg0, arg1, arg2) {
            let dispatcher = this._dispatcher[evt];
            if (typeof dispatcher !== "undefined") {
                dispatcher.dispatch(arg0, arg1, arg2);
            }
        }
    }
    exports.EventDispatcher = EventDispatcher;
});
/*
https://github.com/marijnz/unity-autocomplete-search-field
*/
define("plover/editor/auto_completion_field", ["require", "exports", "UnityEditor", "UnityEditor.IMGUI.Controls", "UnityEngine", "plover/events/dispatcher"], function (require, exports, UnityEditor_1, UnityEditor_IMGUI_Controls_1, UnityEngine_1, dispatcher_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.AutoCompletionField = void 0;
    let Styles = {
        resultHeight: 20,
        resultsBorderWidth: 2,
        resultsMargin: 15,
        resultsLabelOffset: 2,
        entryEven: UnityEngine_1.GUIStyle.op_Implicit("CN EntryBackEven"),
        entryOdd: UnityEngine_1.GUIStyle.op_Implicit("CN EntryBackOdd"),
        labelStyle: new UnityEngine_1.GUIStyle(UnityEditor_1.EditorStyles.label),
        resultsBorderStyle: UnityEngine_1.GUIStyle.op_Implicit("hostview"),
    };
    Styles.labelStyle.alignment = UnityEngine_1.TextAnchor.MiddleLeft;
    Styles.labelStyle.richText = true;
    class AutoCompletionField extends dispatcher_1.EventDispatcher {
        constructor() {
            super();
            this.searchString = "";
            this.maxResults = 15;
            this.results = [];
            this.selectedIndex = -1;
            this.previousMousePosition = UnityEngine_1.Vector2.zero;
            this.selectedIndexByMouse = false;
            this.showResults = false;
        }
        addResult(result) {
            this.results.push(result);
        }
        clearResults() {
            this.results.splice(0);
        }
        onToolbarGUI() {
            this.draw(true);
        }
        onGUI() {
            this.draw(false);
        }
        draw(asToolbar) {
            let rect = UnityEngine_1.GUILayoutUtility.GetRect(1, 1, 18, 18, UnityEngine_1.GUILayout.ExpandWidth(true));
            UnityEngine_1.GUILayout.BeginHorizontal();
            this.doSearchField(rect, asToolbar);
            UnityEngine_1.GUILayout.EndHorizontal();
            rect.y += 18;
            this.doResults(rect);
        }
        doSearchField(rect, asToolbar) {
            if (this.searchField == null) {
                this.searchField = new UnityEditor_IMGUI_Controls_1.SearchField();
                this.searchField.downOrUpArrowKeyPressed("add", this.onDownOrUpArrowKeyPressed.bind(this));
            }
            var result = asToolbar
                ? this.searchField.OnToolbarGUI(rect, this.searchString)
                : this.searchField.OnGUI(rect, this.searchString);
            if (typeof result === "string") {
                if (result != this.searchString) {
                    this.dispatch("change", result);
                    this.selectedIndex = -1;
                    this.showResults = true;
                }
                this.searchString = result;
                if (this.hasSearchbarFocused()) {
                    this.repaintFocusedWindow();
                }
            }
        }
        onDownOrUpArrowKeyPressed() {
            let current = UnityEngine_1.Event.current;
            if (current.keyCode == UnityEngine_1.KeyCode.UpArrow) {
                current.Use();
                this.selectedIndex--;
                this.selectedIndexByMouse = false;
            }
            else {
                current.Use();
                this.selectedIndex++;
                this.selectedIndexByMouse = false;
            }
            if (this.selectedIndex >= this.results.length)
                this.selectedIndex = this.results.length - 1;
            else if (this.selectedIndex < 0)
                this.selectedIndex = -1;
        }
        doResults(rect) {
            if (this.results.length <= 0 || !this.showResults)
                return;
            var current = UnityEngine_1.Event.current;
            rect.height = Styles.resultHeight * Math.min(this.maxResults, this.results.length);
            rect.x = Styles.resultsMargin;
            rect.width -= Styles.resultsMargin * 2;
            var elementRect = new UnityEngine_1.Rect(rect);
            rect.height += Styles.resultsBorderWidth;
            UnityEngine_1.GUI.Label(rect, "", Styles.resultsBorderStyle);
            var mouseIsInResultsRect = rect.Contains(current.mousePosition);
            if (mouseIsInResultsRect) {
                this.repaintFocusedWindow();
            }
            var movedMouseInRect = UnityEngine_1.Vector2.op_Inequality(this.previousMousePosition, current.mousePosition);
            elementRect.x += Styles.resultsBorderWidth;
            elementRect.width -= Styles.resultsBorderWidth * 2;
            elementRect.height = Styles.resultHeight;
            var didJustSelectIndex = false;
            for (var i = 0; i < this.results.length && i < this.maxResults; i++) {
                if (current.type == UnityEngine_1.EventType.Repaint) {
                    var style = i % 2 == 0 ? Styles.entryOdd : Styles.entryEven;
                    style.Draw(elementRect, false, false, i == this.selectedIndex, false);
                    var labelRect = new UnityEngine_1.Rect(elementRect);
                    labelRect.x += Styles.resultsLabelOffset;
                    UnityEngine_1.GUI.Label(labelRect, this.results[i], Styles.labelStyle);
                }
                if (elementRect.Contains(current.mousePosition)) {
                    if (movedMouseInRect) {
                        this.selectedIndex = i;
                        this.selectedIndexByMouse = true;
                        didJustSelectIndex = true;
                    }
                    if (current.type == UnityEngine_1.EventType.MouseDown) {
                        this.onConfirm(this.results[i]);
                    }
                }
                elementRect.y += Styles.resultHeight;
            }
            if (current.type == UnityEngine_1.EventType.Repaint && !didJustSelectIndex && !mouseIsInResultsRect && this.selectedIndexByMouse) {
                this.selectedIndex = -1;
            }
            if ((UnityEngine_1.GUIUtility.hotControl != this.searchField.searchFieldControlID && UnityEngine_1.GUIUtility.hotControl > 0)
                || (current.rawType == UnityEngine_1.EventType.MouseDown && !mouseIsInResultsRect)) {
                this.showResults = false;
            }
            if (current.type == UnityEngine_1.EventType.KeyUp && current.keyCode == UnityEngine_1.KeyCode.Return && this.selectedIndex >= 0) {
                this.onConfirm(this.results[this.selectedIndex]);
            }
            if (current.type == UnityEngine_1.EventType.Repaint) {
                this.previousMousePosition = current.mousePosition;
            }
        }
        onConfirm(result) {
            this.searchString = result;
            this.dispatch("confirm", result);
            this.dispatch("change", result);
            this.repaintFocusedWindow();
            UnityEngine_1.GUIUtility.keyboardControl = 0; // To avoid Unity sometimes not updating the search field text
        }
        hasSearchbarFocused() {
            return UnityEngine_1.GUIUtility.keyboardControl == this.searchField.searchFieldControlID;
        }
        repaintFocusedWindow() {
            if (UnityEditor_1.EditorWindow.focusedWindow != null) {
                UnityEditor_1.EditorWindow.focusedWindow.Repaint();
            }
        }
    }
    exports.AutoCompletionField = AutoCompletionField;
});
define("plover/runtime/serialize", ["require", "exports", "UnityEngine"], function (require, exports, UnityEngine_2) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.RegisterSerializer = exports.GetSerializer = exports.GetLatestSerializer = exports.SerializedTypeID = void 0;
    var SerializedTypeID;
    (function (SerializedTypeID) {
        SerializedTypeID[SerializedTypeID["Null"] = 0] = "Null";
        SerializedTypeID[SerializedTypeID["UserDefinedMin"] = 1] = "UserDefinedMin";
        SerializedTypeID[SerializedTypeID["UserDefinedMax"] = 100] = "UserDefinedMax";
        SerializedTypeID[SerializedTypeID["Array"] = 101] = "Array";
        SerializedTypeID[SerializedTypeID["Object"] = 102] = "Object";
    })(SerializedTypeID = exports.SerializedTypeID || (exports.SerializedTypeID = {}));
    let _PrimitiveSerializerImpls = [];
    let _LatestSerializer;
    function GetLatestSerializer() {
        return _LatestSerializer;
    }
    exports.GetLatestSerializer = GetLatestSerializer;
    function GetSerializer(dataFormat) {
        return _PrimitiveSerializerImpls[dataFormat];
    }
    exports.GetSerializer = GetSerializer;
    function RegisterSerializer(dataFormat, description, types, bSetAsLatest) {
        let impl = {
            dataFormat: dataFormat,
            description: description,
            types: types,
            typeids: [],
        };
        for (let typename in types) {
            let type = types[typename];
            console.assert(type.typeid >= SerializedTypeID.UserDefinedMin && type.typeid <= SerializedTypeID.UserDefinedMax, "typeid must be greater than 0 and less than 100");
            impl.typeids[type.typeid] = type;
        }
        _PrimitiveSerializerImpls[dataFormat] = impl;
        if (!!bSetAsLatest) {
            _LatestSerializer = impl;
        }
    }
    exports.RegisterSerializer = RegisterSerializer;
    RegisterSerializer(1, "v1: without size check", {
        "bool": {
            typeid: 1,
            defaultValue: false,
            serialize(context, buffer, value) {
                buffer.WriteBoolean(!!value);
            },
            deserilize(context, buffer) {
                return buffer.ReadBoolean();
            }
        },
        "float": {
            typeid: 2,
            defaultValue: 0,
            serialize(context, buffer, value) {
                if (typeof value === "number") {
                    buffer.WriteSingle(value);
                }
                else {
                    buffer.WriteSingle(0);
                }
            },
            deserilize(context, buffer) {
                return buffer.ReadSingle();
            }
        },
        "double": {
            typeid: 3,
            defaultValue: 0,
            serialize(context, buffer, value) {
                if (typeof value === "number") {
                    buffer.WriteDouble(value);
                }
                else {
                    buffer.WriteDouble(0);
                }
            },
            deserilize(context, buffer) {
                return buffer.ReadDouble();
            }
        },
        "string": {
            typeid: 4,
            defaultValue: null,
            serialize(context, buffer, value) {
                if (typeof value === "string") {
                    buffer.WriteString(value);
                }
                else {
                    buffer.WriteString(null);
                }
            },
            deserilize(context, buffer) {
                return buffer.ReadString();
            }
        },
        "int": {
            typeid: 5,
            defaultValue: 0,
            serialize(context, buffer, value) {
                if (typeof value === "number") {
                    buffer.WriteInt32(value);
                }
                else {
                    buffer.WriteInt32(0);
                }
            },
            deserilize(context, buffer) {
                return buffer.ReadInt32();
            }
        },
        "uint": {
            typeid: 6,
            defaultValue: 0,
            serialize(context, buffer, value) {
                if (typeof value === "number") {
                    buffer.WriteUInt32(value);
                }
                else {
                    buffer.WriteUInt32(0);
                }
            },
            deserilize(context, buffer) {
                return buffer.ReadUInt32();
            }
        },
        "Vector2": {
            typeid: 7,
            defaultValue: () => UnityEngine_2.Vector2.zero,
            serialize(context, buffer, value) {
                if (value instanceof UnityEngine_2.Vector2) {
                    buffer.WriteSingle(value.x);
                    buffer.WriteSingle(value.y);
                }
                else {
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                }
            },
            deserilize(context, buffer) {
                return new UnityEngine_2.Vector2(buffer.ReadSingle(), buffer.ReadSingle());
            }
        },
        "Vector3": {
            typeid: 8,
            defaultValue: () => UnityEngine_2.Vector3.zero,
            serialize(context, buffer, value) {
                if (value instanceof UnityEngine_2.Vector3) {
                    buffer.WriteSingle(value.x);
                    buffer.WriteSingle(value.y);
                    buffer.WriteSingle(value.z);
                }
                else {
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                }
            },
            deserilize(context, buffer) {
                return new UnityEngine_2.Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
            }
        },
        "Vector4": {
            typeid: 9,
            defaultValue: () => UnityEngine_2.Vector4.zero,
            serialize(context, buffer, value) {
                if (value instanceof UnityEngine_2.Vector4) {
                    buffer.WriteSingle(value.x);
                    buffer.WriteSingle(value.y);
                    buffer.WriteSingle(value.z);
                    buffer.WriteSingle(value.w);
                }
                else {
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                }
            },
            deserilize(context, buffer) {
                return new UnityEngine_2.Vector4(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
            }
        },
        "Rect": {
            typeid: 10,
            defaultValue: () => UnityEngine_2.Rect.zero,
            serialize(context, buffer, value) {
                if (value instanceof UnityEngine_2.Rect) {
                    buffer.WriteSingle(value.x);
                    buffer.WriteSingle(value.y);
                    buffer.WriteSingle(value.width);
                    buffer.WriteSingle(value.height);
                }
                else {
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                }
            },
            deserilize(context, buffer) {
                return new UnityEngine_2.Rect(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
            }
        },
        "Quaternion": {
            typeid: 11,
            defaultValue: () => UnityEngine_2.Quaternion.identity,
            serialize(context, buffer, value) {
                if (value instanceof UnityEngine_2.Quaternion) {
                    buffer.WriteSingle(value.x);
                    buffer.WriteSingle(value.y);
                    buffer.WriteSingle(value.z);
                    buffer.WriteSingle(value.w);
                }
                else {
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(0);
                    buffer.WriteSingle(1);
                }
            },
            deserilize(context, buffer) {
                return new UnityEngine_2.Quaternion(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
            }
        },
        "object": {
            typeid: 12,
            defaultValue: null,
            serialize(context, buffer, value) {
                if (value instanceof UnityEngine_2.Object) {
                    let index = context.ps.AddReferencedObject(value);
                    buffer.WriteInt32(index);
                }
                else {
                    if (!!value) {
                        console.error("only types inheriting UnityEngine.Object is unsupported", value);
                    }
                    buffer.WriteInt32(-1);
                }
            },
            deserilize(context, buffer) {
                let index = buffer.ReadInt32();
                return context.ps.GetReferencedObject(index);
            }
        },
        // js Uint8ArrayBuffer
        "Uint8ArrayBuffer": {
            typeid: 13,
            defaultValue: null,
            serialize(context, buffer, value) {
                if (value instanceof Uint8Array) {
                    let length = value.byteLength;
                    buffer.WriteInt32(length);
                    for (let i = 0; i < length; ++i) {
                        buffer.WriteByte(value[i]);
                    }
                }
                else {
                    buffer.WriteInt32(-1);
                }
            },
            deserilize(context, buffer) {
                let length = buffer.ReadInt32();
                if (length < 0) {
                    return null;
                }
                else {
                    let items = new Uint8Array(length);
                    for (let i = 0; i < length; ++i) {
                        items[i] = buffer.ReadUByte();
                    }
                    return items;
                }
            }
        },
        "json": {
            typeid: 14,
            defaultValue: null,
            serialize(context, buffer, value) {
                if (typeof value === "object") {
                    let json = JSON.stringify(value);
                    buffer.WriteString(json);
                }
                else {
                    buffer.WriteString(null);
                }
            },
            deserilize(context, buffer) {
                let json = buffer.ReadString();
                if (typeof json === "string") {
                    return JSON.parse(json);
                }
                return null;
            }
        },
    }, true);
});
define("plover/runtime/class_decorators", ["require", "exports", "UnityEngine", "plover/runtime/serialize"], function (require, exports, UnityEngine_3, serialize_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.SerializationUtil = exports.ScriptFunction = exports.ScriptProperty = exports.ScriptObject = exports.ScriptString = exports.ScriptNumber = exports.ScriptInteger = exports.ScriptType = exports.ScriptAsset = exports.ScriptSerializable = void 0;
    let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
    let Symbol_MemberFuncs = Symbol.for("MemberFuncs");
    let Symbol_SerializedFields = Symbol.for("SerializedFields");
    function ScriptSerializable(meta) {
        return ScriptType(meta);
    }
    exports.ScriptSerializable = ScriptSerializable;
    function ScriptAsset(meta) {
        return ScriptType(meta);
    }
    exports.ScriptAsset = ScriptAsset;
    // expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
    function ScriptType(meta) {
        return function (target) {
            let OnBeforeSerialize = target.prototype["OnBeforeSerialize"];
            target.prototype["OnBeforeSerialize"] = function (ps) {
                this[Symbol_PropertiesTouched] = false;
                if (typeof OnBeforeSerialize === "function") {
                    OnBeforeSerialize.call(this, ps);
                }
                if (!this[Symbol_PropertiesTouched]) {
                    SerializationUtil.serialize(this, ps);
                }
            };
            let OnAfterDeserialize = target.prototype["OnAfterDeserialize"];
            target.prototype["OnAfterDeserialize"] = function (ps, buffer) {
                this[Symbol_PropertiesTouched] = false;
                if (typeof OnAfterDeserialize === "function") {
                    OnAfterDeserialize.call(this, ps, buffer);
                }
                if (!this[Symbol_PropertiesTouched]) {
                    SerializationUtil.deserialize(this, ps, buffer);
                }
            };
            return target;
        };
    }
    exports.ScriptType = ScriptType;
    function ScriptInteger(meta) {
        let meta_t = meta;
        if (typeof meta_t === "undefined") {
            meta_t = { type: "int" };
        }
        else {
            meta_t.type = "int";
        }
        return ScriptProperty(meta_t);
    }
    exports.ScriptInteger = ScriptInteger;
    function ScriptNumber(meta) {
        let meta_t = meta;
        if (typeof meta_t === "undefined") {
            meta_t = { type: "float" };
        }
        else {
            meta_t.type = "float";
        }
        return ScriptProperty(meta_t);
    }
    exports.ScriptNumber = ScriptNumber;
    function ScriptString(meta) {
        let meta_t = meta;
        if (typeof meta_t === "undefined") {
            meta_t = { type: "string" };
        }
        else {
            meta_t.type = "string";
        }
        return ScriptProperty(meta_t);
    }
    exports.ScriptString = ScriptString;
    function ScriptObject(meta) {
        let meta_t = meta;
        if (typeof meta_t === "undefined") {
            meta_t = { type: "object" };
        }
        else {
            meta_t.type = "object";
        }
        return ScriptProperty(meta_t);
    }
    exports.ScriptObject = ScriptObject;
    function ScriptProperty(meta) {
        return function (target, propertyKey) {
            let slots = target[Symbol_SerializedFields];
            if (typeof slots === "undefined") {
                slots = target[Symbol_SerializedFields] = {};
            }
            let slot = slots[propertyKey] = meta || { type: "object" };
            slot.propertyKey = propertyKey;
            if (typeof slot.serializable !== "boolean") {
                slot.serializable = true;
            }
            if (typeof slot.editable !== "boolean") {
                slot.editable = true;
            }
            if (typeof slot.visible !== "boolean") {
                slot.visible = true;
            }
            if (typeof slot.name !== "string") {
                slot.name = propertyKey;
            }
        };
    }
    exports.ScriptProperty = ScriptProperty;
    function ScriptFunction(meta) {
        return function (target, propertyKey) {
            let funcMap = target[Symbol_MemberFuncs];
            if (typeof funcMap === "undefined") {
                funcMap = target[Symbol_MemberFuncs] = {};
            }
            funcMap[propertyKey] = propertyKey;
        };
    }
    exports.ScriptFunction = ScriptFunction;
    class SerializationUtil {
        static forEach(target, cb) {
            let slots = target[Symbol_SerializedFields];
            if (typeof slots !== "undefined") {
                for (let propertyKey in slots) {
                    cb(slots, propertyKey);
                }
            }
        }
        // 当不需要默认行为时, 调用此函数将序列化状态标记为已完成, 以便跳过默认的 serialize/deserialize 行为
        static markAsReady(target) {
            target[Symbol_PropertiesTouched] = true;
        }
        static serialize(target, ps) {
            SerializationUtil.markAsReady(target);
            let impl = serialize_1.GetLatestSerializer();
            console.assert(typeof ps === "object");
            if (typeof impl === "object") {
                ps.dataFormat = impl.dataFormat;
                let slots = target[Symbol_SerializedFields];
                if (typeof slots !== "undefined") {
                    let buffer = SerializationUtil._serializeObject({ impl: impl, ps: ps }, target, slots);
                    ps.Flush(buffer);
                }
            }
        }
        static _serializeValue(context, slot, value, buffer) {
            let slotType = slot.type;
            let isArray = value instanceof Array;
            if (typeof value === "object") {
                if (value instanceof UnityEngine_3.Object) {
                    slotType = "object";
                }
            }
            if (typeof slotType === "string") {
                // primitive serializer impl
                let s = context.impl.types[slotType];
                if (typeof s === "object") {
                    if (isArray) {
                        let section = SerializationUtil._serializePrimitiveArray(context, s, value);
                        buffer.WriteByte(serialize_1.SerializedTypeID.Array);
                        buffer.WriteByte(s.typeid);
                        buffer.WriteInt32(section.readableBytes);
                        buffer.WriteBytes(section);
                    }
                    else {
                        buffer.WriteByte(s.typeid);
                        s.serialize(context, buffer, value);
                    }
                }
                else {
                    console.error("no serializer impl for", slotType);
                }
            }
            else {
                // typeof slot.type === "function" (a constructor)
                // nested value
                let fieldSlots = slotType.prototype[Symbol_SerializedFields];
                if (typeof fieldSlots !== "undefined") {
                    if (isArray) {
                        let section = SerializationUtil._serializeObjectArray(context, fieldSlots, value);
                        buffer.WriteByte(serialize_1.SerializedTypeID.Array);
                        buffer.WriteByte(serialize_1.SerializedTypeID.Object);
                        buffer.WriteInt32(section.readableBytes);
                        buffer.WriteBytes(section);
                    }
                    else {
                        let section = SerializationUtil._serializeObject(context, value, fieldSlots);
                        buffer.WriteByte(serialize_1.SerializedTypeID.Object);
                        buffer.WriteInt32(section.readableBytes);
                        buffer.WriteBytes(section);
                    }
                }
                else {
                    console.error("no serialization info on field", slot.name);
                }
            }
        }
        static _serializeObjectArray(context, slots, value) {
            let length = value.length;
            let buffer = context.ps.AllocByteBuffer();
            for (let i = 0; i < length; ++i) {
                let section = SerializationUtil._serializeObject(context, value[i], slots);
                buffer.WriteInt32(section.readableBytes);
                buffer.WriteBytes(section);
            }
            return buffer;
        }
        static _serializePrimitiveArray(context, s, value) {
            let length = value.length;
            let buffer = context.ps.AllocByteBuffer();
            for (let i = 0; i < length; ++i) {
                s.serialize(context, buffer, value[i]);
            }
            return buffer;
        }
        static _serializeObject(context, target, slots) {
            let buffer = context.ps.AllocByteBuffer();
            for (let propertyKey in slots) {
                let slot = slots[propertyKey];
                if (slot.serializable) {
                    let value = target && target[propertyKey];
                    // skip undefined and null value
                    if (value == null) {
                        continue;
                    }
                    buffer.WriteString(slot.name);
                    SerializationUtil._serializeValue(context, slot, value, buffer);
                }
            }
            return buffer;
        }
        static deserialize(target, ps, buffer) {
            SerializationUtil.markAsReady(target);
            let slots = target[Symbol_SerializedFields];
            if (typeof slots !== "undefined") {
                let dataFormat = ps.dataFormat || 0;
                let impl = serialize_1.GetSerializer(dataFormat);
                if (typeof impl === "object") {
                    SerializationUtil._deserializeObjectInternal({ impl: impl, ps: ps }, target, slots, buffer);
                }
                else {
                    if (buffer.readableBytes > 0 && ps.dataFormat >= 0) {
                        console.error("no serializer for dataFormat", dataFormat);
                    }
                }
            }
        }
        static _deserializeObject(context, slot, buffer) {
            if (typeof slot.type === "function") {
                let fieldValue = Object.create(slot.type);
                let fieldSlots = slot.type.prototype[Symbol_SerializedFields];
                SerializationUtil._deserializeObjectInternal(context, fieldValue, fieldSlots, buffer);
                return fieldValue;
            }
            else {
                console.error("expecting object but got primitive", slot.type);
            }
        }
        static _deserializeObjectArray(context, slot, buffer) {
            let items = [];
            while (buffer.readableBytes > 0) {
                let size = buffer.ReadInt32();
                let section = buffer.Slice(size);
                let value = SerializationUtil._deserializeObject(context, slot, section);
                items.push(value);
            }
            return items;
        }
        static _deserializePrimitiveArray(context, s, buffer) {
            let items = [];
            while (buffer.readableBytes > 0) {
                let value = s.deserilize(context, buffer);
                items.push(value);
            }
            return items;
        }
        static _deserializeObjectInternal(context, target, slots, buffer) {
            let slotByName = {};
            for (let propertyKey in slots) {
                let slot = slots[propertyKey];
                if (slot.serializable) {
                    slotByName[slot.name] = slot;
                    if (typeof slot.type === "string") {
                        let defaultValue = context.impl.types[slot.type].defaultValue;
                        if (typeof defaultValue === "function") {
                            defaultValue = defaultValue();
                        }
                        target[slot.propertyKey] = defaultValue;
                    }
                    else {
                        target[slot.propertyKey] = null;
                    }
                }
            }
            while (buffer.readableBytes > 0) {
                let name = buffer.ReadString();
                let typeid = buffer.ReadUByte();
                let slot = slotByName[name];
                // should always read the buffer since the serialized field may be removed from script
                let s = context.impl.typeids[typeid];
                if (typeof s === "object") {
                    let slot_value = s.deserilize(context, buffer);
                    if (slot) {
                        if (typeof slot.type === "string") {
                            console.assert(typeid == context.impl.types[slot.type].typeid, "slot type mismatch");
                        }
                        else {
                            if (typeof slot_value === "object") {
                                console.assert(slot_value instanceof slot.type, "slot type mismatch");
                            }
                        }
                        target[slot.propertyKey] = slot_value;
                    }
                    else {
                        console.warn("failed to read slot", name);
                    }
                }
                else {
                    switch (typeid) {
                        case serialize_1.SerializedTypeID.Object: {
                            let size = buffer.ReadInt32();
                            let section = buffer.Slice(size);
                            target[slot.propertyKey] = SerializationUtil._deserializeObject(context, slot, section);
                            break;
                        }
                        case serialize_1.SerializedTypeID.Array: {
                            let elementTypeID = buffer.ReadUByte();
                            let size = buffer.ReadInt32();
                            let section = buffer.Slice(size);
                            let s = context.impl.typeids[elementTypeID];
                            if (typeof s === "undefined") {
                                target[slot.propertyKey] = SerializationUtil._deserializeObjectArray(context, slot, section);
                            }
                            else {
                                target[slot.propertyKey] = SerializationUtil._deserializePrimitiveArray(context, s, section);
                            }
                            break;
                        }
                        case serialize_1.SerializedTypeID.Null: break;
                        default: {
                            console.error(`no serializer for serialized field ${name} with typeid ${typeid}`);
                            break;
                        }
                    }
                }
            }
        }
    }
    exports.SerializationUtil = SerializationUtil;
});
define("plover/editor/drawer", ["require", "exports", "UnityEditor", "UnityEngine"], function (require, exports, UnityEditor_2, UnityEngine_4) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.DefaultPropertyDrawers = void 0;
    exports.DefaultPropertyDrawers = {
        "bool": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = !!rawValue;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.Toggle(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.Toggle(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "int": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || 0;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.IntField(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.IntField(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "float": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || 0;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.FloatField(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.FloatField(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "double": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || 0;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.FloatField(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.FloatField(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "string": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || "";
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.TextField(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.TextField(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "object": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue instanceof UnityEngine_4.Object || null;
                if (editablePE) {
                    let allowSceneObjects = prop.extra && prop.extra.allowSceneObjects;
                    let newValue = UnityEditor_2.EditorGUILayout.ObjectField(label, oldValue, prop.extra && prop.extra.type || Object, typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.ObjectField(label, oldValue, Object, false);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "Vector2": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || UnityEngine_4.Vector2.zero;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.Vector2Field(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.Vector2Field(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "Vector3": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || UnityEngine_4.Vector3.zero;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.Vector3Field(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.Vector3Field(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "Vector4": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || UnityEngine_4.Vector4.zero;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.Vector4Field(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.Vector4Field(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
        "Quaternion": {
            draw(rawValue, prop, label, editablePE) {
                let oldValue = rawValue || UnityEngine_4.Quaternion.identity;
                if (editablePE) {
                    let newValue = UnityEditor_2.EditorGUILayout.Vector4Field(label, oldValue);
                    return newValue;
                }
                else {
                    UnityEditor_2.EditorGUI.BeginDisabledGroup(true);
                    UnityEditor_2.EditorGUILayout.Vector4Field(label, oldValue);
                    UnityEditor_2.EditorGUI.EndDisabledGroup();
                }
            },
        },
    };
});
define("plover/editor/editor_decorators", ["require", "exports", "UnityEditor", "UnityEngine", "plover/runtime/class_decorators", "plover/editor/drawer"], function (require, exports, UnityEditor_3, UnityEngine_5, class_decorators_1, drawer_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.EditorUtil = exports.DefaultEditor = exports.ScriptEditorWindow = exports.ScriptEditor = void 0;
    let Symbol_CustomEditor = Symbol.for("CustomEditor");
    function ScriptEditor(forType) {
        return function (editorType) {
            forType.prototype[Symbol_CustomEditor] = editorType;
            return editorType;
        };
    }
    exports.ScriptEditor = ScriptEditor;
    function ScriptEditorWindow(meta) {
        return class_decorators_1.ScriptType(meta);
    }
    exports.ScriptEditorWindow = ScriptEditorWindow;
    class DefaultEditor extends UnityEditor_3.Editor {
        OnInspectorGUI() {
            EditorUtil.draw(this.target);
        }
    }
    exports.DefaultEditor = DefaultEditor;
    class EditorUtil {
        static getCustomEditor(forType) {
            return forType[Symbol_CustomEditor] || DefaultEditor;
        }
        /**
         * 默认编辑器绘制行为
         */
        static draw(target) {
            class_decorators_1.SerializationUtil.forEach(target, (slots, propertyKey) => {
                let slot = slots[propertyKey];
                if (slot.visible) {
                    let label = slot.label || propertyKey;
                    let editablePE = slot.editable && (!slot.editorOnly || !UnityEditor_3.EditorApplication.isPlaying);
                    if (typeof slot.type === "string") {
                        let d = drawer_1.DefaultPropertyDrawers[slot.type];
                        if (typeof d !== "undefined") {
                            let propertyKey = slot.propertyKey;
                            let oldValue = target[propertyKey];
                            if (oldValue instanceof Array) {
                                let length = oldValue.length;
                                for (let i = 0; i < length; i++) {
                                    let newValue = d.draw(oldValue[i], slot, label, editablePE);
                                    if (editablePE && oldValue[i] != newValue) {
                                        oldValue[i] = newValue;
                                        UnityEditor_3.EditorUtility.SetDirty(target);
                                    }
                                }
                                if (editablePE) {
                                    if (UnityEngine_5.GUILayout.Button("Add Element")) {
                                        oldValue.push(null);
                                        UnityEditor_3.EditorUtility.SetDirty(target);
                                    }
                                }
                            }
                            else {
                                let newValue = d.draw(oldValue, slot, label, editablePE);
                                if (editablePE && oldValue != newValue) {
                                    target[propertyKey] = newValue;
                                    UnityEditor_3.EditorUtility.SetDirty(target);
                                }
                            }
                            return true;
                        }
                        else {
                            UnityEditor_3.EditorGUILayout.LabelField(label);
                            UnityEditor_3.EditorGUILayout.HelpBox("no draw operation for this type", UnityEditor_3.MessageType.Warning);
                        }
                    }
                    else {
                        UnityEditor_3.EditorGUILayout.LabelField(label);
                        UnityEditor_3.EditorGUILayout.HelpBox("unsupported type", UnityEditor_3.MessageType.Warning);
                    }
                }
            });
        }
    }
    exports.EditorUtil = EditorUtil;
});
define("plover/editor/file_watcher", ["require", "exports", "plover/events/dispatcher"], function (require, exports, dispatcher_2) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.FileWatcher = exports.EFileState = void 0;
    var EFileState;
    (function (EFileState) {
        EFileState[EFileState["CHANGE"] = 1] = "CHANGE";
        EFileState[EFileState["NEW"] = 2] = "NEW";
        EFileState[EFileState["DELETE"] = 3] = "DELETE";
    })(EFileState = exports.EFileState || (exports.EFileState = {}));
    class FileWatcher {
        constructor(path, filter) {
            this._dispatcher = new dispatcher_2.EventDispatcher();
            this._disposed = false;
            this._pending = false;
            this._cache = {};
            this._fsw = new FSWatcher(path, filter);
            this._fsw.oncreate = this.oncreate.bind(this);
            this._fsw.onchange = this.onchange.bind(this);
            this._fsw.ondelete = this.ondelete.bind(this);
            this._fsw.includeSubdirectories = true;
            this._fsw.enableRaisingEvents = true;
        }
        get includeSubdirectories() {
            return this._fsw.includeSubdirectories;
        }
        set includeSubdirectories(v) {
            this._fsw.includeSubdirectories = v;
        }
        get enableRaisingEvents() {
            return this._fsw.enableRaisingEvents;
        }
        set enableRaisingEvents(v) {
            this._fsw.enableRaisingEvents = v;
        }
        dispose() {
            if (this._disposed) {
                return;
            }
            this._disposed = true;
            this._fsw.dispose();
            this._fsw = null;
        }
        on(name, caller, fn) {
            this._dispatcher.on(name, caller, fn);
        }
        off(name, caller, fn) {
            this._dispatcher.off(name, caller, fn);
        }
        oncreate(name, fullPath) {
            this.setCacheState(name, fullPath, EFileState.NEW);
        }
        onchange(name, fullPath) {
            this.setCacheState(name, fullPath, EFileState.CHANGE);
        }
        ondelete(name, fullPath) {
            this.setCacheState(name, fullPath, EFileState.DELETE);
        }
        setCacheState(name, fullPath, state) {
            if (this._disposed) {
                return;
            }
            this._cache[name] = {
                name: name,
                fullPath: fullPath,
                state: state,
            };
            if (!this._pending) {
                this._pending = true;
                setTimeout(() => this.dispatchEvents(), 500);
            }
        }
        dispatchEvents() {
            if (this._disposed) {
                return;
            }
            this._pending = false;
            let map = this._cache;
            this._cache = {};
            for (let name in map) {
                let state = map[name];
                this._dispatcher.dispatch(name, state);
                this._dispatcher.dispatch(FileWatcher.ANY, state);
            }
            this._dispatcher.dispatch(FileWatcher.CHANGED, map);
        }
    }
    exports.FileWatcher = FileWatcher;
    FileWatcher.ANY = "* ANY";
    FileWatcher.CHANGED = "* CHANGED";
});
define("plover/editor/js_console", ["require", "exports", "UnityEditor", "UnityEngine", "plover/editor/auto_completion_field"], function (require, exports, UnityEditor_4, UnityEngine_6, auto_completion_field_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.JSConsole = exports.fillAutoCompletion = void 0;
    function fillAutoCompletion(scope, pattern) {
        let result = [];
        if (typeof pattern !== "string") {
            return result;
        }
        let head = '';
        pattern.replace(/\\W*([\\w\\.]+)$/, (a, b, c) => {
            head = pattern.substr(0, c + a.length - b.length);
            pattern = b;
            return b;
        });
        let index = pattern.lastIndexOf('.');
        let left = '';
        if (index >= 0) {
            left = pattern.substr(0, index + 1);
            try {
                scope = eval(pattern.substr(0, index));
            }
            catch (e) {
                scope = null;
            }
            pattern = pattern.substr(index + 1);
        }
        for (let k in scope) {
            if (k.indexOf(pattern) == 0) {
                result.push(head + left + k);
            }
        }
        return result;
    }
    exports.fillAutoCompletion = fillAutoCompletion;
    class JSConsole extends UnityEditor_4.EditorWindow {
        constructor() {
            super(...arguments);
            this._searchField = new auto_completion_field_1.AutoCompletionField();
            this._history = [];
        }
        Awake() {
            this._searchField.on("change", this, this.onSearchChange);
            this._searchField.on("confirm", this, this.onSearchConfirm);
        }
        onSearchChange(s) {
            this._searchField.clearResults();
            fillAutoCompletion(globalThis, s).forEach(element => {
                if (element != s) {
                    this._searchField.addResult(element);
                }
            });
        }
        onSearchConfirm(s) {
            console.log("confirm:", s);
        }
        OnEnable() {
            this.titleContent = new UnityEngine_6.GUIContent("Javascript Console");
        }
        OnGUI() {
            let evt = UnityEngine_6.Event.current;
            this._searchField.onGUI();
            if (evt.type == UnityEngine_6.EventType.KeyUp) {
                switch (evt.keyCode) {
                    case UnityEngine_6.KeyCode.Return: {
                        let code = this._searchField.searchString;
                        if (code != null && code.length > 0) {
                            try {
                                let rval = eval(code);
                                console.log(JSON.stringify(rval));
                            }
                            catch (e) {
                                console.error(e);
                            }
                            // this._history.push(code);
                        }
                        break;
                    }
                }
            }
            // GUI.Box(new Rect(0, 50, 300, 100), this._history.join("\n"));
        }
    }
    exports.JSConsole = JSConsole;
});
define("plover/text/string_utils", ["require", "exports", "jsb"], function (require, exports, jsb) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.StringUtil = exports.Contextual = exports.TEXT = exports.DefaultMatcher = void 0;
    exports.DefaultMatcher = /\$\{([^\{^\}]+)\}/g;
    let _stringCache = new Set();
    function TEXT(s) {
        if (!_stringCache.has(s)) {
            _stringCache.add(s);
            jsb.AddCacheString(s);
        }
        return s;
    }
    exports.TEXT = TEXT;
    /**
     * 按一个上下文对象替换字符串中相应的关键字匹配串
     */
    class Contextual {
        constructor(re = exports.DefaultMatcher) {
            this.re = re;
        }
        /**
         * 绑定一个上下文对象 （通常是一个map）
         */
        bind(context) {
            this.context = context;
            return this;
        }
        /**
         * 替换字符串匹配串 (可以传入上下文，否则使用绑定的)
         * context 可以是一个支持嵌套数据的map, 也可以是一个处理函数.
         * 当使用处理函数时, 参数为取值key字符串.
         */
        replace(text, context) {
            return text.replace(this.re, substring => {
                let variable = substring.substring(2, substring.length - 1);
                let ctx = context || this.context;
                if (typeof ctx == "function") {
                    return ctx(variable);
                }
                else {
                    let elements = variable.split(".");
                    for (let i = 0; i < elements.length; i++) {
                        let element = elements[i];
                        ctx = ctx[element];
                    }
                    return ctx;
                }
            });
        }
        /**
         * let data = {
         *     name: "test",
         *     company: {
         *         name: "unity",
         *     },
         * }
         * let x = Contextual.replace("hello, mm: ${name} ${company.name}", data)
         * console.log(x);
         */
        static replace(text, context, re = exports.DefaultMatcher) {
            return new Contextual(re).replace(text, context);
        }
    }
    exports.Contextual = Contextual;
    class StringUtil {
        /**
         * 替换字符串中的字串
         * @param oldString 原字符串
         * @param matchString 匹配字串
         * @param replaceString 替换字串
         * @returns 替换后的字符串
         */
        static replaceAll(oldString, matchString, replaceString) {
            return oldString.replace(new RegExp(matchString, "g"), replaceString);
        }
        static contains(str, match) {
            return str.search(new RegExp(match, "i")) > 0;
        }
        /**
         * 数字 => 前缀0固定长度字符串
         */
        static prefix(num, length) {
            let n = num.toString();
            let p = length - n.length;
            if (p <= 0) {
                return n;
            }
            return Array(p + 1).join('0') + n;
        }
        /**
         * 转换为固定小数和整数部分长度的字符串
         */
        static prefix2(num, length1, length2) {
            let p = num.toString().split(".");
            if (p.length == 1) {
                return StringUtil.prefix(p[0], length1) + "." + StringUtil.prefix(0, length2);
            }
            return StringUtil.prefix(p[0], length1) + "." + StringUtil.prefix(p[1].substring(0, length2), length2);
        }
        /**
         * 简单字符串表示的时长 (mm:ss.mmm)
         */
        static time(deltaTime) {
            let nmsec = deltaTime % 999;
            let fsec = Math.floor(deltaTime / 1000);
            let nsec = fsec % 60;
            let fmin = Math.floor(fsec / 60);
            let text = fmin < 10 ? "0" + fmin : fmin.toString();
            text += nsec < 10 ? ":0" + nsec : ":" + nsec;
            text += nmsec < 10 ? ".00" + nmsec : (nmsec < 100 ? ".0" + nmsec : "." + nmsec);
            return text;
        }
    }
    exports.StringUtil = StringUtil;
});
define("plover/editor/base/menu_builder", ["require", "exports", "UnityEditor", "UnityEngine"], function (require, exports, UnityEditor_5, UnityEngine_7) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.MenuBuilder = exports.MenuDisabledAction = exports.MenuAction = exports.MenuSeparator = exports.MenuAbstractItem = void 0;
    class MenuAbstractItem {
        constructor(name) {
            this._name = name;
        }
        get name() { return this._name; }
    }
    exports.MenuAbstractItem = MenuAbstractItem;
    class MenuSeparator extends MenuAbstractItem {
        build(menu) {
            menu.AddSeparator(this.name);
        }
    }
    exports.MenuSeparator = MenuSeparator;
    class MenuAction extends MenuAbstractItem {
        constructor(name, action) {
            super(name);
            this._action = action;
        }
        get action() { return this._action; }
        build(menu) {
            let content = new UnityEngine_7.GUIContent(this.name);
            menu.AddItem(content, false, () => this._action());
        }
    }
    exports.MenuAction = MenuAction;
    class MenuDisabledAction extends MenuAbstractItem {
        constructor(name) {
            super(name);
        }
        build(menu) {
            let content = new UnityEngine_7.GUIContent(this.name);
            menu.AddDisabledItem(content, false);
        }
    }
    exports.MenuDisabledAction = MenuDisabledAction;
    class MenuBuilder {
        constructor() {
            this._items = [];
        }
        addAction(name, action, isDisabled = false) {
            if (isDisabled) {
                return this.addDisabledAction(name);
            }
            this._items.push(new MenuAction(name, action));
        }
        addDisabledAction(name) {
            this._items.push(new MenuDisabledAction(name));
        }
        addSeperator() {
            this._items.push(new MenuSeparator(""));
        }
        build() {
            let count = this._items.length;
            if (count > 0) {
                let menu = new UnityEditor_5.GenericMenu();
                for (let i = 0; i < count; i++) {
                    let item = this._items[i];
                    item.build(menu);
                }
                return menu;
            }
            return null;
        }
    }
    exports.MenuBuilder = MenuBuilder;
});
define("plover/editor/base/splitview", ["require", "exports", "UnityEditor", "UnityEngine"], function (require, exports, UnityEditor_6, UnityEngine_8) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.HSplitView = void 0;
    class HSplitView {
        constructor() {
            this.init = false; // 是否初始化
            this.splitPivot = 0;
            this.resize = false;
            this.cursorChangeRect = UnityEngine_8.Rect.zero;
            this.cursorHintRect = UnityEngine_8.Rect.zero;
            this.cursorHintSize = 2;
            this.cursorSize = 6;
            this.cursorHintColor = new UnityEngine_8.Color(0, 0, 0, 0.25);
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
            UnityEditor_6.EditorGUI.DrawRect(this.cursorHintRect, this.cursorHintColor);
            UnityEditor_6.EditorGUIUtility.AddCursorRect(this.cursorChangeRect, UnityEditor_6.MouseCursor.ResizeHorizontal);
            if (UnityEngine_8.Event.current.type == UnityEngine_8.EventType.MouseDown && this.cursorChangeRect.Contains(UnityEngine_8.Event.current.mousePosition)) {
                this.resize = true;
            }
            if (this.resize) {
                let y = this.cursorChangeRect.y;
                let h = this.cursorChangeRect.height;
                this.splitPivot = Math.min(Math.max(UnityEngine_8.Event.current.mousePosition.x, 10), fullWidth - 10);
                this.cursorChangeRect.Set(this.splitPivot - 2, y, this.cursorSize, h);
                this.cursorHintRect.Set(this.splitPivot - 2, y, this.cursorHintSize, h);
                window.Repaint();
            }
            if (UnityEngine_8.Event.current.type == UnityEngine_8.EventType.MouseUp) {
                this.resize = false;
            }
        }
    }
    exports.HSplitView = HSplitView;
});
define("plover/editor/base/treeview", ["require", "exports", "UnityEditor", "UnityEngine", "plover/events/dispatcher", "plover/editor/base/treenode"], function (require, exports, UnityEditor_7, UnityEngine_9, dispatcher_3, treenode_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.UTreeView = void 0;
    class UTreeView {
        constructor(handler) {
            this.SKIP_RETURN = 0;
            this._eventUsed = false;
            this._skipReturn = 0;
            this._indentSize = 16;
            this._rowRect = UnityEngine_9.Rect.zero;
            this._indentRect = UnityEngine_9.Rect.zero;
            this._tempRect = UnityEngine_9.Rect.zero;
            this._editing = false;
            this._deferredMenuPopup = false;
            this._selectionColor = new UnityEngine_9.Color(44 / 255, 93 / 255, 135 / 255);
            this._rowColor = new UnityEngine_9.Color(0.5, 0.5, 0.5, 0.1);
            this._focusColor = new UnityEngine_9.Color(58 / 255, 121 / 255, 187 / 255);
            this._debug_touchChild = 0;
            this._debug_drawChild = 0;
            this._searchString = "";
            this._handler = handler;
            this._root = new treenode_1.UTreeNode(this, null, true, "/");
            this._root.isEditable = false;
            this._root.isSearchable = false;
            this._root.expanded = true;
        }
        get selected() { return this._selected; }
        set selected(value) {
            var _a;
            if (this._selected != value) {
                (_a = this._selected) === null || _a === void 0 ? void 0 : _a.endEdit();
                this._editing = false;
                this._skipReturn = 0;
                this._selected = value;
            }
        }
        get searchString() { return this._searchString; }
        set searchString(value) { this.search(value); }
        get root() {
            return this._root;
        }
        get handler() { return this._handler; }
        set handler(value) { this._handler = value; }
        on(evt, caller, fn) {
            if (!this._events) {
                this._events = new dispatcher_3.EventDispatcher();
            }
            this._events.on(evt, caller, fn);
        }
        off(evt, caller, fn) {
            if (this._events) {
                this._events.off(evt, caller, fn);
            }
        }
        dispatch(name, arg0, arg1, arg2) {
            if (!this._events) {
                this._events = new dispatcher_3.EventDispatcher();
            }
            this._events.dispatch(name, arg0, arg1, arg2);
        }
        allocFolderHierarchy(path, data) {
            return this._getFolderHierarchy(path, data);
        }
        getFolderHierarchy(path) {
            return this._getFolderHierarchy(path, null);
        }
        _getFolderHierarchy(path, data) {
            if (path.startsWith("/")) {
                path = path.substring(1);
            }
            let node = this._root;
            if (path.length > 0) {
                let hierarchy = path.split("/");
                for (let i = 0; i < hierarchy.length; i++) {
                    node = node.getFolderByName(hierarchy[i], true, data);
                }
            }
            return node;
        }
        removeAll() {
            this._root.removeAll();
            this.selected = null;
        }
        deleteNode(node) {
            if (node && this._selected == node && node.parent) {
                this._selected = this.findNextNode(node) || this.findPreviousNode(node);
                return node.parent.removeChild(node);
            }
            return false;
        }
        search(p) {
            if (p == null) {
                p = "";
            }
            if (this._searchString != p) {
                this._searchString = p;
                this._search(this._root);
            }
        }
        _search(node) {
            node.match(this._searchString);
            for (let i = 0, count = node.childCount; i < count; i++) {
                this._search(node.getChildByIndex(i));
            }
        }
        expandAll() { this._root.expandAll(); }
        collapseAll() { this._root.collapseAll(); }
        draw(offsetX, offsetY, width, height) {
            var _a;
            let repaint = false;
            let cEvent = UnityEngine_9.Event.current;
            if (this._deferredMenuPopup) {
                this._deferredMenuPopup = false;
                if (this._selected) {
                    this._selected.drawMenu(this, cEvent.mousePosition, this._handler);
                    repaint = true;
                }
            }
            this._debug_touchChild = 0;
            this._debug_drawChild = 0;
            this._eventUsed = false;
            this._height = 0;
            this._drawY = 0;
            this._rowIndex = 0;
            if (this._searchString == null || this._searchString.length == 0) {
                this.calcRowHeight(this._root);
                this.setControlRect(cEvent);
                this.drawRow(this._root, 0, offsetY, height);
            }
            else {
                this.calcSearchResultsHeight(this._root);
                this.setControlRect(cEvent);
                this.drawSearchResults(this._root, 0, offsetY, height);
            }
            if (this._controlID == UnityEngine_9.GUIUtility.keyboardControl) {
                this._tempRect.Set(0, 0, 1, height);
                UnityEditor_7.EditorGUI.DrawRect(this._tempRect, this._focusColor);
            }
            if (cEvent.isKey) {
                let eventType = cEvent.type;
                if (this._editing) {
                    switch (eventType) {
                        case UnityEngine_9.EventType.KeyUp:
                            {
                                let keyCode = cEvent.keyCode;
                                if (keyCode == UnityEngine_9.KeyCode.Return) {
                                    if (this._skipReturn > 0) {
                                        this._skipReturn--;
                                        this.useEvent();
                                    }
                                    else {
                                        UnityEngine_9.GUI.FocusControl(null);
                                        UnityEngine_9.GUIUtility.keyboardControl = this._controlID;
                                        (_a = this._selected) === null || _a === void 0 ? void 0 : _a.endEdit();
                                        this._editing = false;
                                        this._skipReturn = 0;
                                        this.useEvent();
                                    }
                                }
                            }
                            break;
                    }
                }
                else {
                    if (this._selected && this._controlEventType == UnityEngine_9.EventType.KeyUp && this._controlID == UnityEngine_9.GUIUtility.keyboardControl) {
                        // console.log(GUIUtility.keyboardControl, this._controlID);
                        let keyCode = cEvent.keyCode;
                        if (keyCode == UnityEngine_9.KeyCode.Return) {
                            if (this._selected.isEditable) {
                                this._editing = true;
                                this._skipReturn = this.SKIP_RETURN;
                                this.useEvent();
                            }
                        }
                        else {
                            if (keyCode == UnityEngine_9.KeyCode.UpArrow) {
                                if (this._selected.parent) {
                                    let sibling = this.findPreviousNode(this._selected);
                                    if (sibling) {
                                        this._selected = sibling;
                                        this._selected.expandUp();
                                        this.useEvent();
                                    }
                                }
                            }
                            else if (keyCode == UnityEngine_9.KeyCode.DownArrow) {
                                let sibling = this.findNextNode(this._selected);
                                if (sibling) {
                                    this._selected = sibling;
                                    this._selected.expandUp();
                                    this.useEvent();
                                }
                            }
                            else if (keyCode == UnityEngine_9.KeyCode.LeftArrow) {
                                if (this._selected.expanded && this._selected.isFolder) {
                                    this._selected.expanded = false;
                                    this._selected.expandUp();
                                }
                                else if (this._selected.parent) {
                                    this._selected = this._selected.parent;
                                    this._selected.expandUp();
                                }
                                this.useEvent();
                            }
                            else if (keyCode == UnityEngine_9.KeyCode.RightArrow) {
                                this._selected.expanded = true;
                                this._selected.expandUp();
                                this.useEvent();
                            }
                        }
                    }
                }
            }
            else {
                if (!this._editing && this._controlEventType == UnityEngine_9.EventType.MouseUp) {
                    this._tempRect.Set(0, 0, width, height);
                    if (this._tempRect.Contains(this._controlMousePos)) {
                        UnityEngine_9.GUIUtility.keyboardControl = this._controlID;
                        repaint = true;
                    }
                }
            }
            return this._deferredMenuPopup || repaint;
        }
        calcRowHeight(node) {
            this._height += node.calcRowHeight();
            if (node.expanded) {
                for (let i = 0, count = node.childCount; i < count; i++) {
                    this.calcRowHeight(node.getChildByIndex(i));
                }
            }
        }
        calcSearchResultsHeight(node) {
            if (node.isMatch) {
                this._height += node.calcRowHeight();
            }
            for (let i = 0, count = node.childCount; i < count; i++) {
                this.calcRowHeight(node.getChildByIndex(i));
            }
        }
        setControlRect(cEvent) {
            this._controlRect = UnityEditor_7.EditorGUILayout.GetControlRect(false, this._height, UnityEngine_9.GUILayout.MinWidth(160));
            this._controlID = UnityEngine_9.GUIUtility.GetControlID(UnityEngine_9.FocusType.Keyboard, this._controlRect);
            this._controlEventType = cEvent.GetTypeForControl(this._controlID);
            if (this._controlEventType == UnityEngine_9.EventType.MouseUp) {
                this._controlMousePos = cEvent.mousePosition;
            }
        }
        useEvent() {
            this._eventUsed = true;
            UnityEngine_9.GUI.changed = true;
            UnityEngine_9.Event.current.Use();
        }
        drawSearchResults(node, depth, offsetY, height) {
            let drawY = this._drawY;
            if (node.isMatch) {
                this._drawY += node.height;
                ++this._rowIndex;
                ++this._debug_touchChild;
                if ((this._drawY - offsetY) > 0 && (drawY - offsetY) < height) {
                    let rowIndent = 0;
                    let baseX = 14;
                    let bSelected = this._selected == node;
                    ++this._debug_drawChild;
                    this._rowRect.Set(this._controlRect.x, this._controlRect.y + drawY, this._controlRect.width, node.height);
                    this._indentRect.Set(this._controlRect.x + baseX + rowIndent, this._rowRect.y, this._controlRect.width - rowIndent, node.height);
                    if (bSelected) {
                        UnityEditor_7.EditorGUI.DrawRect(this._rowRect, this._selectionColor);
                    }
                    else if (this._rowIndex % 2) {
                        UnityEditor_7.EditorGUI.DrawRect(this._rowRect, this._rowColor);
                    }
                    node.draw(this._indentRect, bSelected, bSelected && this._editing, this._indentSize);
                    if (this._controlEventType == UnityEngine_9.EventType.MouseUp) {
                        if (this._rowRect.Contains(this._controlMousePos)) {
                            if (UnityEngine_9.Event.current.button == 1) {
                                if (this._selected == node) {
                                    node.drawMenu(this, this._controlMousePos, this._handler);
                                    this.useEvent();
                                }
                                else {
                                    this.selected = node;
                                    if (!this._editing) {
                                        this._deferredMenuPopup = true;
                                    }
                                    this.useEvent();
                                }
                            }
                            else if (UnityEngine_9.Event.current.button == 0) {
                                if (node.isFolder && node._foldoutRect.Contains(this._controlMousePos)) {
                                    node.expanded = !node.expanded;
                                }
                                else {
                                    this.selected = node;
                                }
                                this.useEvent();
                            }
                        }
                    }
                }
            }
            for (let i = 0, count = node.childCount; i < count; i++) {
                this.drawSearchResults(node.getChildByIndex(i), depth + 1, offsetY, height);
            }
        }
        drawRow(node, depth, offsetY, height) {
            let drawY = this._drawY;
            this._drawY += node.height;
            ++this._rowIndex;
            ++this._debug_touchChild;
            if ((this._drawY - offsetY) > 0 && (drawY - offsetY) < height) {
                let rowIndent = this._indentSize * depth;
                let baseX = 14;
                let bSelected = this._selected == node;
                ++this._debug_drawChild;
                this._rowRect.Set(this._controlRect.x, this._controlRect.y + drawY, this._controlRect.width, node.height);
                this._indentRect.Set(this._controlRect.x + baseX + rowIndent, this._rowRect.y, this._controlRect.width - rowIndent, node.height);
                if (bSelected) {
                    UnityEditor_7.EditorGUI.DrawRect(this._rowRect, this._selectionColor);
                }
                else if (this._rowIndex % 2) {
                    UnityEditor_7.EditorGUI.DrawRect(this._rowRect, this._rowColor);
                }
                node.draw(this._indentRect, bSelected, bSelected && this._editing, this._indentSize);
                if (this._controlEventType == UnityEngine_9.EventType.MouseUp) {
                    if (this._rowRect.Contains(this._controlMousePos)) {
                        if (UnityEngine_9.Event.current.button == 1) {
                            if (this._selected == node) {
                                node.drawMenu(this, this._controlMousePos, this._handler);
                                this.useEvent();
                            }
                            else {
                                this.selected = node;
                                if (!this._editing) {
                                    this._deferredMenuPopup = true;
                                }
                                this.useEvent();
                            }
                        }
                        else if (UnityEngine_9.Event.current.button == 0) {
                            if (node.isFolder && node._foldoutRect.Contains(this._controlMousePos)) {
                                node.expanded = !node.expanded;
                            }
                            else {
                                this.selected = node;
                            }
                            this.useEvent();
                        }
                    }
                }
            }
            else {
                node.visible = false;
            }
            if (node.expanded) {
                for (let i = 0, count = node.childCount; i < count; i++) {
                    this.drawRow(node.getChildByIndex(i), depth + 1, offsetY, height);
                    // if (this._drawLine && i == count - 1) {
                    //     this._point.Set(child._lineStart.x, node._lineStart.y, 0);
                    //     // Handles.DrawDottedLine(this._point, child._lineStartIn, 1);
                    //     Handles.color = Color.gray;
                    //     Handles.DrawLine(this._point, child._lineStartIn);
                    // }
                }
            }
        }
        findPreviousNode(node) {
            let sibling = node.parent.findLastSibling(node);
            while (sibling && sibling.expanded && sibling.childCount > 0) {
                sibling = sibling.getLastChild();
            }
            return sibling || node.parent;
        }
        findNextNode(node) {
            if (node.expanded && node.childCount > 0) {
                return node.getFirstChild();
            }
            while (node.parent) {
                let sibling = node.parent.findNextSibling(node);
                if (sibling) {
                    return sibling;
                }
                node = node.parent;
            }
            return null;
        }
    }
    exports.UTreeView = UTreeView;
    UTreeView.CONTEXT_MENU = "CONTEXT_MENU";
});
define("plover/editor/base/treenode", ["require", "exports", "QuickJS.Unity", "UnityEditor", "UnityEngine", "plover/events/dispatcher", "plover/editor/base/menu_builder", "plover/editor/base/treeview"], function (require, exports, QuickJS_Unity_1, UnityEditor_8, UnityEngine_10, dispatcher_4, menu_builder_1, treeview_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.UTreeNode = exports.BuiltinIcons = void 0;
    class BuiltinIcons {
        static getIcon(name) {
            let icon = BuiltinIcons._cache[name];
            if (typeof icon === "undefined") {
                icon = BuiltinIcons._cache[name] = QuickJS_Unity_1.UnityHelper.GetIcon(name);
            }
            return icon;
        }
    }
    exports.BuiltinIcons = BuiltinIcons;
    BuiltinIcons._cache = {};
    class UTreeNode {
        constructor(tree, parent, isFolder, name) {
            this._children = null;
            this._expanded = true;
            this._name = "noname";
            this.isSearchable = true;
            this.isEditable = true;
            // _lineStart = Vector3.zero;
            // _lineStartIn = Vector3.zero;
            // protected _lineEnd = Vector3.zero;
            this._foldoutRect = UnityEngine_10.Rect.zero;
            this._bFocusTextField = false;
            this._bVisible = true;
            this._height = 0;
            this._bMatch = true;
            this._name = name;
            this._tree = tree;
            this._parent = parent;
            this._children = isFolder ? [] : null;
        }
        get isMatch() { return this._bMatch; }
        get height() { return this._height; }
        /**
         * 当前层级是否展开
         */
        get expanded() { return this._expanded; }
        set expanded(value) {
            if (this._expanded != value) {
                this._expanded = value;
            }
        }
        get isFolder() { return !!this._children; }
        get visible() { return this._bVisible; }
        set visible(value) {
            if (this._bVisible != value) {
                this._bVisible = value;
            }
        }
        get parent() { return this._parent; }
        get isRoot() { return this._parent == null; }
        get name() { return this._name; }
        set name(value) {
            if (this._name != value) {
                let oldName = this._name;
                this._name = value;
                this._tree.handler.onTreeNodeNameChanged(this, oldName);
            }
        }
        get fullPath() {
            let path = this._name;
            let node = this._parent;
            while (node && !node.isRoot) {
                if (node._name && node._name.length > 0) {
                    path = node._name + "/" + path;
                }
                node = node._parent;
            }
            return path;
        }
        get treeView() { return this._tree; }
        get childCount() { return this._children ? this._children.length : 0; }
        on(evt, caller, fn) {
            if (!this._events) {
                this._events = new dispatcher_4.EventDispatcher();
            }
            this._events.on(evt, caller, fn);
        }
        off(evt, caller, fn) {
            if (this._events) {
                this._events.off(evt, caller, fn);
            }
        }
        dispatch(name, arg0, arg1, arg2) {
            if (!this._events) {
                this._events = new dispatcher_4.EventDispatcher();
            }
            this._events.dispatch(name, arg0, arg1, arg2);
        }
        match(p) {
            if (p == null || p.length == 0) {
                return this._bMatch = true;
            }
            return this._bMatch = this.isSearchable && this._name.indexOf(p) >= 0;
        }
        getRelativePath(top) {
            let path = this._name;
            let node = this._parent;
            while (node && node != top) {
                path = node._name + "/" + path;
                node = node._parent;
            }
            return path;
        }
        expandAll() {
            this._setExpandAll(true);
        }
        collapseAll() {
            this._setExpandAll(false);
        }
        _setExpandAll(state) {
            this._expanded = state;
            if (this._children) {
                for (let i = 0, count = this._children.length; i < count; i++) {
                    this._children[i]._setExpandAll(state);
                }
            }
        }
        expandUp() {
            let node = this._parent;
            while (node) {
                node.expanded = true;
                node = node.parent;
            }
        }
        /**
         * 获取指定节点的在当前层级中的下一个相邻节点
         */
        findNextSibling(node) {
            if (this._children) {
                let index = this._children.indexOf(node);
                if (index >= 0 && index < this._children.length - 1) {
                    return this._children[index + 1];
                }
            }
            return null;
        }
        /**
         * 获取指定节点的在当前层级中的上一个相邻节点
         */
        findLastSibling(node) {
            if (this._children) {
                let index = this._children.indexOf(node);
                if (index > 0) {
                    return this._children[index - 1];
                }
            }
            return null;
        }
        forEachChild(fn) {
            if (this._children) {
                for (let i = 0, count = this._children.length; i < count; i++) {
                    fn(this._children[i]);
                }
            }
        }
        /**
         * 获取当前层级下的子节点
         * @param index 索引 或者 命名
         * @param autoNew 不存在时是否创建 (仅通过命名获取时有效)
         * @returns 子节点
         */
        getFolderByName(name, isAutoCreate, data) {
            if (this._children) {
                for (let i = 0, size = this._children.length; i < size; i++) {
                    let child = this._children[i];
                    if (child.isFolder && child.name == name) {
                        return child;
                    }
                }
                if (isAutoCreate) {
                    let child = this._addChild(name, true, data);
                    return child;
                }
            }
            return null;
        }
        getLeafByName(name, isAutoCreate, data) {
            if (this._children) {
                for (let i = 0, size = this._children.length; i < size; i++) {
                    let child = this._children[i];
                    if (!child.isFolder && child.name == name) {
                        return child;
                    }
                }
                if (isAutoCreate) {
                    let child = this._addChild(name, false, data);
                    return child;
                }
            }
            return null;
        }
        getChildByIndex(index) {
            return this._children[index];
        }
        /**
         * 当前层级最后一个子节点
         */
        getLastChild() {
            return this._children && this._children.length > 0 ? this._children[this._children.length - 1] : null;
        }
        /**
         * 当前层级第一个子节点
         */
        getFirstChild() {
            return this._children && this._children.length > 0 ? this._children[0] : null;
        }
        addFolderChild(name) {
            return this.getFolderByName(name, true, null);
        }
        addLeafChild(name) {
            return this.getLeafByName(name, true, null);
        }
        allocLeafChild(name, data) {
            return this.getLeafByName(name, true, data);
        }
        /**
         * 在当前层级添加一个子节点
         */
        _addChild(name, isFolder, data) {
            if (this._children) {
                let node = new UTreeNode(this._tree, this, isFolder, name);
                this._children.push(node);
                node._expanded = true;
                node.data = data;
                this._tree.handler.onTreeNodeCreated(node);
                return node;
            }
            return null;
        }
        /**
         * 将一个子节点从当前层级中移除
         */
        removeChild(node) {
            if (this._children) {
                let index = this._children.indexOf(node);
                if (index >= 0) {
                    this._children.splice(index, 1);
                    return true;
                }
            }
            return false;
        }
        removeAll() {
            if (this._children) {
                this._children.splice(0);
            }
        }
        calcRowHeight() {
            this._height = UnityEditor_8.EditorGUIUtility.singleLineHeight;
            return this._height;
        }
        drawMenu(treeView, pos, handler) {
            let builder = new menu_builder_1.MenuBuilder();
            handler.onTreeNodeContextMenu(this, builder);
            treeView.dispatch(treeview_1.UTreeView.CONTEXT_MENU, builder, this);
            let menu = builder.build();
            if (menu) {
                menu.ShowAsContext();
            }
        }
        draw(rect, bSelected, bEditing, indentSize) {
            // let lineY = rect.y + rect.height * 0.5;
            // this._lineStartIn.Set(rect.x - indentSize * 1.5, lineY, 0);
            // this._lineStart.Set(rect.x - indentSize * 1.5, rect.y + rect.height, 0);
            this._bVisible = true;
            if (this._children && this._children.length > 0) {
                this._foldoutRect.Set(rect.x - 14, rect.y, 12, rect.height);
                /*this._expanded =*/ UnityEditor_8.EditorGUI.Foldout(this._foldoutRect, this._expanded, UnityEngine_10.GUIContent.none);
                // this._lineEnd.Set(rect.x - indentSize, lineY, 0);
                let image = this._expanded ? BuiltinIcons.getIcon("FolderOpened") : BuiltinIcons.getIcon("Folder");
                if (!this._label) {
                    this._label = new UnityEngine_10.GUIContent(this._name, image);
                }
                else {
                    this._label.image = image;
                }
            }
            else {
                // this._lineEnd.Set(rect.x - 4, lineY, 0);
                if (!this._label) {
                    this._label = new UnityEngine_10.GUIContent(this._name, BuiltinIcons.getIcon("JsScript"));
                }
            }
            // Handles.color = Color.gray;
            // Handles.DrawLine(this._lineStartIn, this._lineEnd);
            if (bEditing) {
                let text;
                if (this._bFocusTextField) {
                    UnityEngine_10.GUI.SetNextControlName("TreeViewNode.Editing");
                    this._label.text = UnityEditor_8.EditorGUI.TextField(rect, this._label.text);
                }
                else {
                    UnityEngine_10.GUI.SetNextControlName("TreeViewNode.Editing");
                    this._label.text = UnityEditor_8.EditorGUI.TextField(rect, this._label.text);
                    UnityEngine_10.GUI.FocusControl("TreeViewNode.Editing");
                }
            }
            else {
                this._bFocusTextField = false;
                UnityEditor_8.EditorGUI.LabelField(rect, this._label, bSelected ? UnityEditor_8.EditorStyles.whiteLabel : UnityEditor_8.EditorStyles.label);
            }
        }
        endEdit() {
            if (this._label.text != this._name) {
                this._tree.handler.onTreeNodeNameEditEnded(this, this._label.text);
            }
        }
    }
    exports.UTreeNode = UTreeNode;
});
define("plover/editor/base/breadcrumb", ["require", "exports", "UnityEditor", "UnityEngine", "jsb", "plover/events/dispatcher"], function (require, exports, UnityEditor_9, UnityEngine_11, jsb, dispatcher_5) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.Breadcrumb = void 0;
    class Breadcrumb extends dispatcher_5.EventDispatcher {
        constructor() {
            super();
            this._cache = [];
            this._color = new UnityEngine_11.Color(1, 1, 1, 0);
            this._sv = UnityEngine_11.Vector2.zero;
            jsb.AddCacheString(">");
            this._height = UnityEditor_9.EditorGUIUtility.singleLineHeight + 14;
            this._heightOptionSV = UnityEngine_11.GUILayout.Height(this._height);
            this._heightOptionHB = UnityEngine_11.GUILayout.Height(this._height - 6);
        }
        get height() { return this._height; }
        draw(node) {
            if (!node || !node.parent) {
                return;
            }
            let count = 0;
            while (node.parent) {
                this._cache[count++] = node;
                node = node.parent;
            }
            this._sv = UnityEditor_9.EditorGUILayout.BeginScrollView(this._sv, this._heightOptionSV);
            UnityEngine_11.GUILayout.BeginHorizontal(this._heightOptionHB);
            let color = UnityEngine_11.GUI.backgroundColor;
            UnityEngine_11.GUI.backgroundColor = this._color;
            for (let i = count - 1; i >= 0; --i) {
                let item = this._cache[i];
                if (UnityEngine_11.GUILayout.Button(item.name, UnityEngine_11.GUILayout.ExpandWidth(false))) {
                    this.dispatch(Breadcrumb.CLICKED, item, false);
                }
                if (i != 0) {
                    // GUILayout.Label(">", GUILayout.ExpandWidth(false));
                    if (UnityEngine_11.GUILayout.Button(">", UnityEngine_11.GUILayout.ExpandWidth(false))) {
                        this.dispatch(Breadcrumb.CLICKED, item, true);
                    }
                    // let rect = EditorGUILayout.GetControlRect(GUILayout.Width(10));
                    // EditorGUI.DrawRect(rect, Color.yellow);
                }
                this._cache[i] = null;
            }
            UnityEngine_11.GUI.backgroundColor = color;
            UnityEngine_11.GUILayout.EndHorizontal();
            UnityEditor_9.EditorGUILayout.EndScrollView();
        }
    }
    exports.Breadcrumb = Breadcrumb;
    Breadcrumb.CLICKED = "CLICKED";
});
define("plover/editor/base/editor_window_base", ["require", "exports", "UnityEditor", "UnityEngine", "plover/editor/base/menu_builder", "plover/editor/base/splitview", "plover/editor/base/treenode", "plover/editor/base/treeview", "jsb", "plover/text/string_utils", "plover/editor/base/breadcrumb"], function (require, exports, UnityEditor_10, UnityEngine_12, menu_builder_2, splitview_1, treenode_2, treeview_2, jsb, string_utils_1, breadcrumb_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.EditorWindowBase = void 0;
    class EditorWindowBase extends UnityEditor_10.EditorWindow {
        constructor() {
            super(...arguments);
            this._treeViewScroll = UnityEngine_12.Vector2.zero;
            this._toolbarRect = UnityEngine_12.Rect.zero;
            this._leftRect = UnityEngine_12.Rect.zero;
            this._rightRect = UnityEngine_12.Rect.zero;
            // protected _topSplitLine = Rect.zero;
            this._searchLabel = new UnityEngine_12.GUIContent("Search");
            this._tempRect = UnityEngine_12.Rect.zero;
            this._contents = {};
            this.toobarHeight = 24;
        }
        onTreeNodeNameEditEnded(node, newName) {
            if (node.isRoot || !node.data) {
                return;
            }
            node.name = newName;
        }
        onTreeNodeNameChanged(node, oldName) {
            if (node.isRoot || !node.data) {
                return;
            }
        }
        onTreeNodeCreated(node) {
            if (node.data) {
                return;
            }
        }
        onTreeNodeContextMenu(node, builder) {
            if (!node.isRoot) {
            }
        }
        buildBreadcrumbMenu(top, node, builder) {
            node.forEachChild(child => {
                let relativePath = child.getRelativePath(top);
                builder.addAction(relativePath, () => {
                    this._treeView.selected = child;
                });
                this.buildBreadcrumbMenu(top, child, builder);
            });
        }
        onClickBreadcrumb(node, isContext) {
            if (isContext) {
                let builder = new menu_builder_2.MenuBuilder();
                this.buildBreadcrumbMenu(node, node, builder);
                let menu = builder.build();
                if (menu) {
                    menu.ShowAsContext();
                }
            }
            else {
                this._treeView.selected = node;
            }
        }
        Awake() {
            jsb.AddCacheString("");
            this._hSplitView = new splitview_1.HSplitView();
            this._treeView = new treeview_2.UTreeView(this);
            this._breadcrumb = new breadcrumb_1.Breadcrumb();
            this._breadcrumb.on(breadcrumb_1.Breadcrumb.CLICKED, this, this.onClickBreadcrumb);
        }
        drawLeftTreeView(width, height) {
            this._treeView.searchString = UnityEditor_10.EditorGUILayout.TextField(this._treeView.searchString);
            this._treeViewScroll = UnityEditor_10.EditorGUILayout.BeginScrollView(this._treeViewScroll);
            if (this._treeView.draw(this._treeViewScroll.x, this._treeViewScroll.y, width, height)) {
                this.Repaint();
            }
            UnityEditor_10.EditorGUILayout.EndScrollView();
        }
        drawConfigView(data, node) { }
        drawFolderView(data, node) { }
        TRect(x, y, w, h) {
            this._tempRect.Set(x, y, w, h);
            return this._tempRect;
        }
        TContent(name, icon, tooltip, text) {
            let content = this._contents[name];
            if (typeof content === "undefined") {
                if (typeof text !== "string") {
                    text = name;
                }
                if (typeof tooltip === "string") {
                    content = new UnityEngine_12.GUIContent(text, treenode_2.BuiltinIcons.getIcon(icon), tooltip);
                }
                else {
                    content = new UnityEngine_12.GUIContent(text, treenode_2.BuiltinIcons.getIcon(icon));
                }
                this._contents[name] = content;
            }
            return content;
        }
        OnGUI() {
            this._event = UnityEngine_12.Event.current;
            let padding = 8;
            let windowStartY = this.toobarHeight + padding * 0.5;
            let windowWidth = this.position.width;
            let windowHeight = this.position.height - windowStartY;
            this._toolbarRect.Set(padding * 0.5, padding * 0.5, windowWidth - padding, this.toobarHeight);
            UnityEngine_12.GUILayout.BeginArea(this._toolbarRect);
            UnityEngine_12.GUILayout.BeginHorizontal();
            this.drawToolBar();
            UnityEngine_12.GUILayout.EndHorizontal();
            UnityEngine_12.GUILayout.EndArea();
            this._tempRect.Set(0, windowStartY, windowWidth, 1);
            UnityEditor_10.EditorGUI.DrawRect(this._tempRect, this._hSplitView.cursorHintColor);
            this._hSplitView.draw(this, windowStartY, windowWidth, windowHeight);
            this._leftRect.Set(0, windowStartY, this._hSplitView.cursorChangeRect.x, windowHeight);
            UnityEngine_12.GUILayout.BeginArea(this._leftRect);
            this.drawLeftTreeView(this._leftRect.width, this._leftRect.height);
            UnityEngine_12.GUILayout.EndArea();
            this._rightRect.Set(this._leftRect.width + this._hSplitView.cursorChangeRect.width + padding, windowStartY + padding, windowWidth - this._hSplitView.cursorChangeRect.xMax - padding * 2, windowHeight - padding * 2 - windowStartY);
            UnityEngine_12.GUILayout.BeginArea(this._rightRect);
            let selected = this._treeView.selected;
            if (selected && selected.data) {
                this._breadcrumb.draw(selected);
                this._tempRect.Set(0, this._breadcrumb.height - 6, this._rightRect.width, 1);
                UnityEditor_10.EditorGUI.DrawRect(this._tempRect, this._hSplitView.cursorHintColor);
                if (selected.isFolder) {
                    this.drawFolderView(selected.data, selected);
                }
                else {
                    this.drawConfigView(selected.data, selected);
                }
            }
            else {
                UnityEditor_10.EditorGUILayout.HelpBox(string_utils_1.TEXT("Nothing Selected"), UnityEditor_10.MessageType.Warning);
            }
            UnityEngine_12.GUILayout.EndArea();
        }
    }
    exports.EditorWindowBase = EditorWindowBase;
});
define("plover/editor/js_reload", ["require", "exports", "plover/editor/file_watcher", "jsb"], function (require, exports, file_watcher_1, jsb_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.reload = void 0;
    let FileWatcherSymbol = Symbol.for("GlobalFileWatcher");
    if (typeof globalThis[FileWatcherSymbol] !== "undefined") {
        globalThis[FileWatcherSymbol].dispose();
        delete globalThis[FileWatcherSymbol];
    }
    let fw = new file_watcher_1.FileWatcher("Scripts", "*.js");
    function reload(mod) {
        if (typeof mod === "object") {
            let dirtylist = [];
            collect_reload_deps(mod, dirtylist);
            do_reload(dirtylist);
        }
    }
    exports.reload = reload;
    function do_reload(dirtylist) {
        if (dirtylist.length > 0) {
            jsb_1.ModuleManager.BeginReload();
            for (let i = 0; i < dirtylist.length; i++) {
                let mod = dirtylist[i];
                console.warn("reloading", mod.id);
                jsb_1.ModuleManager.MarkReload(mod.id);
            }
            jsb_1.ModuleManager.EndReload();
        }
    }
    function collect_reload_deps(mod, dirtylist) {
        if (dirtylist.indexOf(mod) < 0) {
            dirtylist.push(mod);
            let parent = mod.parent;
            if (typeof parent === "object") {
                collect_reload_deps(parent, dirtylist);
                parent = parent.parent;
            }
        }
    }
    fw.on(file_watcher_1.FileWatcher.CHANGED, this, function (filestates) {
        let cache = require.main["cache"];
        let dirtylist = [];
        for (let name in filestates) {
            let filestate = filestates[name];
            // console.log("file changed:", filestate.name, filestate.fullPath, filestate.state);
            if (filestate.state != file_watcher_1.EFileState.CHANGE) {
                continue;
            }
            for (let moduleId in cache) {
                let mod = cache[moduleId];
                // console.warn(mod.filename, mod.filename == filestate.fullPath)
                if (mod.filename == filestate.fullPath) {
                    collect_reload_deps(mod, dirtylist);
                    break;
                }
            }
        }
        do_reload(dirtylist);
    });
    globalThis[FileWatcherSymbol] = fw;
});
define("plover/editor/js_module_view", ["require", "exports", "UnityEditor", "UnityEngine", "plover/text/string_utils", "plover/editor/base/editor_window_base", "plover/editor/js_reload"], function (require, exports, UnityEditor_11, UnityEngine_13, string_utils_2, editor_window_base_1, js_reload_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.JSModuleView = void 0;
    class JSModuleView extends editor_window_base_1.EditorWindowBase {
        Awake() {
            super.Awake();
            if (!this._touch) {
                this.updateModules();
            }
            this.toobarHeight = 26;
        }
        OnEnable() {
            this.titleContent = new UnityEngine_13.GUIContent("JS Modules");
        }
        drawFolderView(data, node) {
            if (!data) {
                return;
            }
            let mod = data;
            UnityEditor_11.EditorGUILayout.Toggle(string_utils_2.TEXT("Main"), mod == require.main);
            UnityEditor_11.EditorGUILayout.BeginHorizontal();
            UnityEditor_11.EditorGUILayout.TextField(string_utils_2.TEXT("Module ID"), mod.id);
            let doReload = false;
            if (mod["resolvername"] != "source") {
                UnityEditor_11.EditorGUI.BeginDisabledGroup(true);
                doReload = UnityEngine_13.GUILayout.Button(string_utils_2.TEXT("Reload"));
                UnityEditor_11.EditorGUI.EndDisabledGroup();
            }
            else {
                doReload = UnityEngine_13.GUILayout.Button(string_utils_2.TEXT("Reload"));
            }
            UnityEditor_11.EditorGUILayout.EndHorizontal();
            UnityEditor_11.EditorGUILayout.TextField(string_utils_2.TEXT("File Name"), mod.filename);
            if (typeof mod.parent === "object") {
                UnityEditor_11.EditorGUILayout.TextField(string_utils_2.TEXT("Parent"), mod.parent.id);
            }
            else {
                UnityEditor_11.EditorGUILayout.TextField(string_utils_2.TEXT("Parent"), string_utils_2.TEXT("TOP LEVEL"));
            }
            if (doReload) {
                js_reload_1.reload(mod);
            }
        }
        drawToolBar() {
            if (UnityEngine_13.GUILayout.Button(this.TContent("Expand All", "Hierarchy", "Expand All"), UnityEditor_11.EditorStyles.toolbarButton, UnityEngine_13.GUILayout.Width(128), UnityEngine_13.GUILayout.Height(this.toobarHeight))) {
                this._treeView.expandAll();
            }
            if (UnityEngine_13.GUILayout.Button(this.TContent("Collapse All", "Collapsed", "Collapse All"), UnityEditor_11.EditorStyles.toolbarButton, UnityEngine_13.GUILayout.Width(128), UnityEngine_13.GUILayout.Height(this.toobarHeight))) {
                this._treeView.collapseAll();
            }
            if (UnityEngine_13.GUILayout.Button(this.TContent("Refresh", "Refresh", "Refresh"), UnityEditor_11.EditorStyles.toolbarButton, UnityEngine_13.GUILayout.Width(128), UnityEngine_13.GUILayout.Height(this.toobarHeight))) {
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
});
define("plover/editor/base/content_cache", ["require", "exports", "UnityEngine"], function (require, exports, UnityEngine_14) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.EdCache = void 0;
    class EdCache {
        static T(title, tooltip = null, image = null) {
            let item = EdCache.cache[title];
            if (typeof item === "undefined") {
                item = EdCache.cache[title] = tooltip == null ? new UnityEngine_14.GUIContent(title, image) : new UnityEngine_14.GUIContent(title, image, tooltip);
            }
            return item;
        }
    }
    exports.EdCache = EdCache;
    EdCache.cache = {};
});
define("plover/events/data_binding", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.DataBinding = exports.Subscribers = exports.Subscriber = void 0;
    class Subscriber {
        constructor(model, key) {
            this._model = model;
            this._key = key;
            this._model.addSubscriber(this);
        }
        get value() {
            return this._model[this._key];
        }
        set value(newValue) {
            this._source = true;
            this._model[this._key] = newValue;
            this._source = false;
        }
        update(value) {
        }
        notify(value) {
            if (!this._source) {
                this.update(value);
            }
        }
        unsubscribe() {
            if (this._model) {
                this._model.removeSubscriber(this);
                this._model = undefined;
            }
        }
    }
    exports.Subscriber = Subscriber;
    class Subscribers {
        notify(valueProxy) {
            if (this._subs) {
                const copy = this._subs.slice();
                for (let i = 0, len = copy.length; i < len; i++) {
                    copy[i].notify(valueProxy);
                }
            }
        }
        addSub(sub) {
            if (!this._subs) {
                this._subs = [];
            }
            this._subs.push(sub);
        }
        removeSub(sub) {
            if (this._subs && this._subs.length) {
                const index = this._subs.indexOf(sub);
                if (index >= 0) {
                    this._subs.splice(index, 1);
                }
            }
        }
        // 废弃当前值, 将监听者转移给新值
        transfer(newValue) {
            newValue._subs = this._subs;
            this._subs = undefined;
        }
    }
    exports.Subscribers = Subscribers;
    const SubscribersKey = Symbol.for("subscribers");
    class DataBinding {
        constructor() {
            Object.defineProperty(this, SubscribersKey, { value: new Subscribers(), enumerable: false });
        }
        addSubscriber(sub) {
            this[SubscribersKey].addSub(sub);
        }
        removeSubscriber(sub) {
            this[SubscribersKey].removeSub(sub);
        }
        static bind(data) {
            let model = new DataBinding();
            let subscribers = model[SubscribersKey];
            for (let key in data) {
                if (key.startsWith("$") || key.startsWith("_$")) {
                    continue;
                }
                let value = data[key];
                let valueProxy = value;
                if (typeof value === "object") {
                    valueProxy = DataBinding.bind(value);
                }
                Object.defineProperty(model, key, {
                    enumerable: true,
                    get() {
                        return valueProxy;
                    },
                    set(newValue) {
                        if (newValue !== value) {
                            let oldValue = value;
                            if (typeof newValue === "object") {
                                valueProxy = DataBinding.bind(newValue);
                                oldValue[SubscribersKey].transfer(valueProxy[SubscribersKey]);
                                // Model.transfer(<Model><any>oldValue, <Model><any>valueProxy);
                            }
                            else {
                                valueProxy = newValue;
                            }
                            subscribers.notify(valueProxy);
                        }
                    },
                });
            }
            return model;
        }
        static subscribe(SubscriberType, modelObject, path, ...args) {
            let model = modelObject;
            let keys = path.split(".");
            let key = path;
            for (let i = 0, len = keys.length - 1; i < len; i++) {
                key = keys[i];
                model = model[key];
            }
            let sub = new SubscriberType(model, key, ...args);
            return sub;
        }
    }
    exports.DataBinding = DataBinding;
});
/*!
 * Vue.js v2.6.14
 * (c) 2014-2021 Evan You
 * Released under the MIT License.
 */
define("plover/jsx/vue", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.ViewModel = void 0;
    /**
     * Perform no operation.
     * Stubbing args to make Flow happy without leaving useless transpiled code
     * with ...rest (https://flow.org/blog/2017/05/07/Strict-Function-Call-Arity/).
     */
    function noop(a, b, c) { }
    function warn(m, v) { }
    // can we use __proto__?
    var hasProto = '__proto__' in {};
    /**
     * Augment a target Object or Array by intercepting
     * the prototype chain using __proto__
     */
    function protoAugment(target, src) {
        /* eslint-disable no-proto */
        target.__proto__ = src;
        /* eslint-enable no-proto */
    }
    /**
     * Augment a target Object or Array by defining
     * hidden properties.
     */
    /* istanbul ignore next */
    function copyAugment(target, src, keys) {
        for (var i = 0, l = keys.length; i < l; i++) {
            var key = keys[i];
            def(target, key, src[key]);
        }
    }
    /**
     * Define a property.
     */
    function def(obj, key, val, enumerable) {
        Object.defineProperty(obj, key, {
            value: val,
            enumerable: !!enumerable,
            writable: true,
            configurable: true
        });
    }
    /*
     * not type checking this file because flow doesn't play well with
     * dynamically accessing methods on Array prototype
     */
    var arrayProto = Array.prototype;
    var arrayMethods = Object.create(arrayProto);
    var methodsToPatch = [
        'push',
        'pop',
        'shift',
        'unshift',
        'splice',
        'sort',
        'reverse'
    ];
    /**
     * Intercept mutating methods and emit events
     */
    methodsToPatch.forEach(function (method) {
        // cache original method
        var original = arrayProto[method];
        def(arrayMethods, method, function mutator() {
            var args = [], len = arguments.length;
            while (len--)
                args[len] = arguments[len];
            var result = original.apply(this, args);
            var ob = this.__ob__;
            var inserted;
            switch (method) {
                case 'push':
                case 'unshift':
                    inserted = args;
                    break;
                case 'splice':
                    inserted = args.slice(2);
                    break;
            }
            if (inserted) {
                ob.observeArray(inserted);
            }
            // notify change
            ob.dep.notify();
            return result;
        });
    });
    /*  */
    var arrayKeys = Object.getOwnPropertyNames(arrayMethods);
    /**
     * Remove an item from an array.
     */
    function remove(arr, item) {
        if (arr.length) {
            var index = arr.indexOf(item);
            if (index > -1) {
                return arr.splice(index, 1);
            }
        }
    }
    /*  */
    var uid = 0;
    /**
     * A dep is an observable that can have multiple
     * directives subscribing to it.
     */
    function Dep() {
        this.id = uid++;
        this.subs = [];
    }
    ;
    Dep.prototype.addSub = function addSub(sub) {
        this.subs.push(sub);
    };
    Dep.prototype.removeSub = function removeSub(sub) {
        remove(this.subs, sub);
    };
    Dep.prototype.depend = function depend() {
        if (Dep.target) {
            Dep.target.addDep(this);
        }
    };
    Dep.prototype.notify = function notify() {
        // stabilize the subscriber list first
        var subs = this.subs.slice();
        for (var i = 0, l = subs.length; i < l; i++) {
            subs[i].update();
        }
    };
    // The current target watcher being evaluated.
    // This is globally unique because only one watcher
    // can be evaluated at a time.
    Dep.target = null;
    var targetStack = [];
    function pushTarget(target) {
        targetStack.push(target);
        Dep.target = target;
    }
    function popTarget() {
        targetStack.pop();
        Dep.target = targetStack[targetStack.length - 1];
    }
    /**
     * Collect dependencies on array elements when the array is touched, since
     * we cannot intercept array element access like property getters.
     */
    function dependArray(value) {
        for (var e = (void 0), i = 0, l = value.length; i < l; i++) {
            e = value[i];
            e && e.__ob__ && e.__ob__.dep.depend();
            if (Array.isArray(e)) {
                dependArray(e);
            }
        }
    }
    /**
     * Define a reactive property on an Object.
     */
    function defineReactive$$1(obj, key, val, customSetter, shallow) {
        var dep = new Dep();
        var property = Object.getOwnPropertyDescriptor(obj, key);
        if (property && property.configurable === false) {
            return;
        }
        // cater for pre-defined getter/setters
        var getter = property && property.get;
        var setter = property && property.set;
        if ((!getter || setter) && arguments.length === 2) {
            val = obj[key];
        }
        var childOb = !shallow && observe(val);
        Object.defineProperty(obj, key, {
            enumerable: true,
            configurable: true,
            get: function reactiveGetter() {
                var value = getter ? getter.call(obj) : val;
                if (Dep.target) {
                    dep.depend();
                    if (childOb) {
                        childOb.dep.depend();
                        if (Array.isArray(value)) {
                            dependArray(value);
                        }
                    }
                }
                return value;
            },
            set: function reactiveSetter(newVal) {
                var value = getter ? getter.call(obj) : val;
                /* eslint-disable no-self-compare */
                if (newVal === value || (newVal !== newVal && value !== value)) {
                    return;
                }
                /* eslint-enable no-self-compare */
                if (customSetter) {
                    customSetter();
                }
                // #7981: for accessor properties without setter
                if (getter && !setter) {
                    return;
                }
                if (setter) {
                    setter.call(obj, newVal);
                }
                else {
                    val = newVal;
                }
                childOb = !shallow && observe(newVal);
                dep.notify();
            }
        });
    }
    /**
     * Check whether an object has the property.
     */
    var hasOwnProperty = Object.prototype.hasOwnProperty;
    function hasOwn(obj, key) {
        return hasOwnProperty.call(obj, key);
    }
    /**
     * Quick object check - this is primarily used to tell
     * Objects from primitive values when we know the value
     * is a JSON-compliant type.
     */
    function isObject(obj) {
        return obj !== null && typeof obj === 'object';
    }
    /**
     * In some cases we may want to disable observation inside a component's
     * update computation.
     */
    var shouldObserve = true;
    function toggleObserving(value) {
        shouldObserve = value;
    }
    /**
     * Get the raw type string of a value, e.g., [object Object].
     */
    var _toString = Object.prototype.toString;
    function toRawType(value) {
        return _toString.call(value).slice(8, -1);
    }
    /**
     * Strict object type check. Only returns true
     * for plain JavaScript objects.
     */
    function isPlainObject(obj) {
        return _toString.call(obj) === '[object Object]';
    }
    function isRegExp(v) {
        return _toString.call(v) === '[object RegExp]';
    }
    /**
     * Attempt to create an observer instance for a value,
     * returns the new observer if successfully observed,
     * or the existing observer if the value already has one.
     */
    function observe(value, asRootData) {
        if (!isObject(value)) {
            return;
        }
        var ob;
        if (hasOwn(value, '__ob__') && value.__ob__ instanceof Observer) {
            ob = value.__ob__;
        }
        else if (shouldObserve &&
            (Array.isArray(value) || isPlainObject(value)) &&
            Object.isExtensible(value) &&
            !value._isVue) {
            ob = new Observer(value);
        }
        if (asRootData && ob) {
            ob.vmCount++;
        }
        return ob;
    }
    /* istanbul ignore next */
    function isNative(Ctor) {
        return typeof Ctor === 'function' && /native code/.test(Ctor.toString());
    }
    var _Set;
    /* istanbul ignore if */ // $flow-disable-line
    if (typeof Set !== 'undefined' && isNative(Set)) {
        // use native Set when available.
        _Set = Set;
    }
    else {
        // a non-standard Set polyfill that only works with primitive keys.
        _Set = /*@__PURE__*/ (function () {
            function Set() {
                this.set = Object.create(null);
            }
            Set.prototype.has = function has(key) {
                return this.set[key] === true;
            };
            Set.prototype.add = function add(key) {
                this.set[key] = true;
            };
            Set.prototype.clear = function clear() {
                this.set = Object.create(null);
            };
            return Set;
        }());
    }
    /*  */
    /**
     * unicode letters used for parsing html tags, component names and property paths.
     * using https://www.w3.org/TR/html53/semantics-scripting.html#potentialcustomelementname
     * skipping \u10000-\uEFFFF due to it freezing up PhantomJS
     */
    var unicodeRegExp = /a-zA-Z\u00B7\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u037D\u037F-\u1FFF\u200C-\u200D\u203F-\u2040\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD/;
    /**
     * Parse simple path.
     */
    var bailRE = new RegExp(("[^" + (unicodeRegExp.source) + ".$_\\d]"));
    function parsePath(path) {
        if (bailRE.test(path)) {
            return;
        }
        var segments = path.split('.');
        return function (obj) {
            for (var i = 0; i < segments.length; i++) {
                if (!obj) {
                    return;
                }
                obj = obj[segments[i]];
            }
            return obj;
        };
    }
    /*  */
    var seenObjects = new _Set();
    /**
     * Recursively traverse an object to evoke all converted
     * getters, so that every nested property inside the object
     * is collected as a "deep" dependency.
     */
    function traverse(val) {
        _traverse(val, seenObjects);
        seenObjects.clear();
    }
    function _traverse(val, seen) {
        var i, keys;
        var isA = Array.isArray(val);
        if ((!isA && !isObject(val)) || Object.isFrozen(val) /* || val instanceof VNode */) {
            return;
        }
        if (val.__ob__) {
            var depId = val.__ob__.dep.id;
            if (seen.has(depId)) {
                return;
            }
            seen.add(depId);
        }
        if (isA) {
            i = val.length;
            while (i--) {
                _traverse(val[i], seen);
            }
        }
        else {
            keys = Object.keys(val);
            i = keys.length;
            while (i--) {
                _traverse(val[keys[i]], seen);
            }
        }
    }
    /*  */
    function handleError(err, vm, info) {
        // Deactivate deps tracking while processing error handler to avoid possible infinite rendering.
        // See: https://github.com/vuejs/vuex/issues/1505
        pushTarget();
        try {
            if (vm) {
                var cur = vm;
                while ((cur = cur.$parent)) {
                    var hooks = cur.$options.errorCaptured;
                    if (hooks) {
                        for (var i = 0; i < hooks.length; i++) {
                            try {
                                var capture = hooks[i].call(cur, err, vm, info) === false;
                                if (capture) {
                                    return;
                                }
                            }
                            catch (e) {
                                globalHandleError(e, cur, 'errorCaptured hook');
                            }
                        }
                    }
                }
            }
            globalHandleError(err, vm, info);
        }
        finally {
            popTarget();
        }
    }
    function isDef(v) {
        return v !== undefined && v !== null;
    }
    function isPromise(val) {
        return (isDef(val) &&
            typeof val.then === 'function' &&
            typeof val.catch === 'function');
    }
    function invokeWithErrorHandling(handler, context, args, vm, info) {
        var res;
        try {
            res = args ? handler.apply(context, args) : handler.call(context);
            if (res && !res._isVue && isPromise(res) && !res._handled) {
                res.catch(function (e) { return handleError(e, vm, info + " (Promise/async)"); });
                // issue #9511
                // avoid catch triggering multiple times when nested calls
                res._handled = true;
            }
        }
        catch (e) {
            handleError(e, vm, info);
        }
        return res;
    }
    function globalHandleError(err, vm, info) {
        // if (config.errorHandler) {
        //     try {
        //         return config.errorHandler.call(null, err, vm, info)
        //     } catch (e) {
        //         // if the user intentionally throws the original error in the handler,
        //         // do not log it twice
        //         if (e !== err) {
        //             logError(e, null, 'config.errorHandler');
        //         }
        //     }
        // }
        logError(err, vm, info);
    }
    function logError(err, vm, info) {
        {
            warn(("Error in " + info + ": \"" + (err.toString()) + "\""), vm);
        }
        /* istanbul ignore else */
        if (typeof console !== 'undefined') {
            console.error(err);
        }
        else {
            throw err;
        }
    }
    /**
     * Push a watcher into the watcher queue.
     * Jobs with duplicate IDs will be skipped unless it's
     * pushed when the queue is being flushed.
     */
    function queueWatcher(watcher) {
        var id = watcher.id;
        if (has[id] == null) {
            has[id] = true;
            if (!flushing) {
                queue.push(watcher);
            }
            else {
                // if already flushing, splice the watcher based on its id
                // if already past its id, it will be run next immediately.
                var i = queue.length - 1;
                while (i > index && queue[i].id > watcher.id) {
                    i--;
                }
                queue.splice(i + 1, 0, watcher);
            }
            // queue the flush
            if (!waiting) {
                waiting = true;
                // if (!config.async) {
                //     flushSchedulerQueue();
                //     return
                // }
                nextTick(flushSchedulerQueue);
            }
        }
    }
    var callbacks = [];
    var pending = false;
    function flushCallbacks() {
        pending = false;
        var copies = callbacks.slice(0);
        callbacks.length = 0;
        for (var i = 0; i < copies.length; i++) {
            copies[i]();
        }
    }
    // Here we have async deferring wrappers using microtasks.
    // In 2.5 we used (macro) tasks (in combination with microtasks).
    // However, it has subtle problems when state is changed right before repaint
    // (e.g. #6813, out-in transitions).
    // Also, using (macro) tasks in event handler would cause some weird behaviors
    // that cannot be circumvented (e.g. #7109, #7153, #7546, #7834, #8109).
    // So we now use microtasks everywhere, again.
    // A major drawback of this tradeoff is that there are some scenarios
    // where microtasks have too high a priority and fire in between supposedly
    // sequential events (e.g. #4521, #6690, which have workarounds)
    // or even between bubbling of the same event (#6566).
    var timerFunc;
    var p = Promise.resolve();
    timerFunc = function () {
        p.then(flushCallbacks);
    };
    function nextTick(cb, ctx) {
        var _resolve;
        callbacks.push(function () {
            if (cb) {
                try {
                    cb.call(ctx);
                }
                catch (e) {
                    handleError(e, ctx, 'nextTick');
                }
            }
            else if (_resolve) {
                _resolve(ctx);
            }
        });
        if (!pending) {
            pending = true;
            timerFunc();
        }
        // $flow-disable-line
        if (!cb && typeof Promise !== 'undefined') {
            return new Promise(function (resolve) {
                _resolve = resolve;
            });
        }
    }
    /*  */
    var MAX_UPDATE_COUNT = 100;
    var queue = [];
    var activatedChildren = [];
    var has = {};
    var circular = {};
    var waiting = false;
    var flushing = false;
    var index = 0;
    /**
     * Reset the scheduler's state.
     */
    function resetSchedulerState() {
        index = queue.length = activatedChildren.length = 0;
        has = {};
        {
            circular = {};
        }
        waiting = flushing = false;
    }
    // Async edge case #6566 requires saving the timestamp when event listeners are
    // attached. However, calling performance.now() has a perf overhead especially
    // if the page has thousands of event listeners. Instead, we take a timestamp
    // every time the scheduler flushes and use that for all event listeners
    // attached during that flush.
    var currentFlushTimestamp = 0;
    // Async edge case fix requires storing an event listener's attach timestamp.
    var getNow = Date.now;
    /**
     * Flush both queues and run the watchers.
     */
    function flushSchedulerQueue() {
        currentFlushTimestamp = getNow();
        flushing = true;
        var watcher, id;
        // Sort queue before flush.
        // This ensures that:
        // 1. Components are updated from parent to child. (because parent is always
        //    created before the child)
        // 2. A component's user watchers are run before its render watcher (because
        //    user watchers are created before the render watcher)
        // 3. If a component is destroyed during a parent component's watcher run,
        //    its watchers can be skipped.
        queue.sort(function (a, b) { return a.id - b.id; });
        // do not cache length because more watchers might be pushed
        // as we run existing watchers
        for (index = 0; index < queue.length; index++) {
            watcher = queue[index];
            if (watcher.before) {
                watcher.before();
            }
            id = watcher.id;
            has[id] = null;
            watcher.run();
            // in dev build, check and stop circular updates.
            if (has[id] != null) {
                circular[id] = (circular[id] || 0) + 1;
                if (circular[id] > MAX_UPDATE_COUNT) {
                    warn('You may have an infinite update loop ' + (watcher.user
                        ? ("in watcher with expression \"" + (watcher.expression) + "\"")
                        : "in a component render function."), watcher.vm);
                    break;
                }
            }
        }
        // keep copies of post queues before resetting state
        var activatedQueue = activatedChildren.slice();
        var updatedQueue = queue.slice();
        resetSchedulerState();
        // call component updated and activated hooks
        callActivatedHooks(activatedQueue);
        callUpdatedHooks(updatedQueue);
        // // devtool hook
        // /* istanbul ignore if */
        // if (devtools && config.devtools) {
        //     devtools.emit('flush');
        // }
    }
    function callActivatedHooks(queue) {
        for (var i = 0; i < queue.length; i++) {
            queue[i]._inactive = true;
            // activateChildComponent(queue[i], true /* true */);
        }
    }
    function callHook(vm, hook) {
        // #7573 disable dep collection when invoking lifecycle hooks
        pushTarget();
        var handlers = vm.$options[hook];
        var info = hook + " hook";
        if (handlers) {
            for (var i = 0, j = handlers.length; i < j; i++) {
                invokeWithErrorHandling(handlers[i], vm, null, vm, info);
            }
        }
        if (vm._hasHookEvent) {
            vm.$emit('hook:' + hook);
        }
        popTarget();
    }
    function callUpdatedHooks(queue) {
        var i = queue.length;
        while (i--) {
            var watcher = queue[i];
            var vm = watcher.vm;
            if (vm._watcher === watcher && vm._isMounted && !vm._isDestroyed) {
                callHook(vm, 'updated');
            }
        }
    }
    /*  */
    var uid$2 = 0;
    /**
     * A watcher parses an expression, collects dependencies,
     * and fires callback when the expression value changes.
     * This is used for both the $watch() api and directives.
     */
    function Watcher(vm, expOrFn, cb, options, isRenderWatcher) {
        this.vm = vm;
        if (isRenderWatcher) {
            vm._watcher = this;
        }
        vm._watchers.push(this);
        // options
        if (options) {
            this.deep = !!options.deep;
            this.user = !!options.user;
            this.lazy = !!options.lazy;
            this.sync = !!options.sync;
            this.before = options.before;
        }
        else {
            this.deep = this.user = this.lazy = this.sync = false;
        }
        this.cb = cb;
        this.id = ++uid$2; // uid for batching
        this.active = true;
        this.dirty = this.lazy; // for lazy watchers
        this.deps = [];
        this.newDeps = [];
        this.depIds = new _Set();
        this.newDepIds = new _Set();
        this.expression = expOrFn.toString();
        // parse expression for getter
        if (typeof expOrFn === 'function') {
            this.getter = expOrFn;
        }
        else {
            this.getter = parsePath(expOrFn);
            if (!this.getter) {
                this.getter = noop;
                warn("Failed watching path: \"" + expOrFn + "\" " +
                    'Watcher only accepts simple dot-delimited paths. ' +
                    'For full control, use a function instead.', vm);
            }
        }
        this.value = this.lazy
            ? undefined
            : this.get();
    }
    ;
    /**
     * Evaluate the getter, and re-collect dependencies.
     */
    Watcher.prototype.get = function get() {
        pushTarget(this);
        var value;
        var vm = this.vm;
        try {
            value = this.getter.call(vm, vm);
        }
        catch (e) {
            if (this.user) {
                handleError(e, vm, ("getter for watcher \"" + (this.expression) + "\""));
            }
            else {
                throw e;
            }
        }
        finally {
            // "touch" every property so they are all tracked as
            // dependencies for deep watching
            if (this.deep) {
                traverse(value);
            }
            popTarget();
            this.cleanupDeps();
        }
        return value;
    };
    /**
     * Add a dependency to this directive.
     */
    Watcher.prototype.addDep = function addDep(dep) {
        var id = dep.id;
        if (!this.newDepIds.has(id)) {
            this.newDepIds.add(id);
            this.newDeps.push(dep);
            if (!this.depIds.has(id)) {
                dep.addSub(this);
            }
        }
    };
    /**
     * Clean up for dependency collection.
     */
    Watcher.prototype.cleanupDeps = function cleanupDeps() {
        var i = this.deps.length;
        while (i--) {
            var dep = this.deps[i];
            if (!this.newDepIds.has(dep.id)) {
                dep.removeSub(this);
            }
        }
        var tmp = this.depIds;
        this.depIds = this.newDepIds;
        this.newDepIds = tmp;
        this.newDepIds.clear();
        tmp = this.deps;
        this.deps = this.newDeps;
        this.newDeps = tmp;
        this.newDeps.length = 0;
    };
    /**
     * Subscriber interface.
     * Will be called when a dependency changes.
     */
    Watcher.prototype.update = function update() {
        /* istanbul ignore else */
        if (this.lazy) {
            this.dirty = true;
        }
        else if (this.sync) {
            this.run();
        }
        else {
            queueWatcher(this);
        }
    };
    /**
     * Scheduler job interface.
     * Will be called by the scheduler.
     */
    Watcher.prototype.run = function run() {
        if (this.active) {
            var value = this.get();
            if (value !== this.value ||
                // Deep watchers and watchers on Object/Arrays should fire even
                // when the value is the same, because the value may
                // have mutated.
                isObject(value) ||
                this.deep) {
                // set new value
                var oldValue = this.value;
                this.value = value;
                if (this.user) {
                    var info = "callback for watcher \"" + (this.expression) + "\"";
                    invokeWithErrorHandling(this.cb, this.vm, [value, oldValue], this.vm, info);
                }
                else {
                    this.cb.call(this.vm, value, oldValue);
                }
            }
        }
    };
    /**
     * Evaluate the value of the watcher.
     * This only gets called for lazy watchers.
     */
    Watcher.prototype.evaluate = function evaluate() {
        this.value = this.get();
        this.dirty = false;
    };
    /**
     * Depend on all deps collected by this watcher.
     */
    Watcher.prototype.depend = function depend() {
        var i = this.deps.length;
        while (i--) {
            this.deps[i].depend();
        }
    };
    /**
     * Remove self from all dependencies' subscriber list.
     */
    Watcher.prototype.teardown = function teardown() {
        if (this.active) {
            // remove self from vm's watcher list
            // this is a somewhat expensive operation so we skip it
            // if the vm is being destroyed.
            if (!this.vm._isBeingDestroyed) {
                remove(this.vm._watchers, this);
            }
            var i = this.deps.length;
            while (i--) {
                this.deps[i].removeSub(this);
            }
            this.active = false;
        }
    };
    /**
     * Observer class that is attached to each observed
     * object. Once attached, the observer converts the target
     * object's property keys into getter/setters that
     * collect dependencies and dispatch updates.
     */
    var Observer = function Observer(value) {
        this.value = value;
        this.dep = new Dep();
        this.vmCount = 0;
        def(value, '__ob__', this);
        if (Array.isArray(value)) {
            if (hasProto) {
                protoAugment(value, arrayMethods);
            }
            else {
                copyAugment(value, arrayMethods, arrayKeys);
            }
            this.observeArray(value);
        }
        else {
            this.walk(value);
        }
    };
    /**
     * Walk through all properties and convert them into
     * getter/setters. This method should only be called when
     * value type is Object.
     */
    Observer.prototype.walk = function walk(obj) {
        var keys = Object.keys(obj);
        for (var i = 0; i < keys.length; i++) {
            defineReactive$$1(obj, keys[i]);
        }
    };
    /**
     * Observe a list of Array items.
     */
    Observer.prototype.observeArray = function observeArray(items) {
        for (var i = 0, l = items.length; i < l; i++) {
            observe(items[i]);
        }
    };
    var validDivisionCharRE = /[\w).+\-_$\]]/;
    function parseFilters(exp) {
        var inSingle = false;
        var inDouble = false;
        var inTemplateString = false;
        var inRegex = false;
        var curly = 0;
        var square = 0;
        var paren = 0;
        var lastFilterIndex = 0;
        var c, prev, i, expression, filters;
        for (i = 0; i < exp.length; i++) {
            prev = c;
            c = exp.charCodeAt(i);
            if (inSingle) {
                if (c === 0x27 && prev !== 0x5C) {
                    inSingle = false;
                }
            }
            else if (inDouble) {
                if (c === 0x22 && prev !== 0x5C) {
                    inDouble = false;
                }
            }
            else if (inTemplateString) {
                if (c === 0x60 && prev !== 0x5C) {
                    inTemplateString = false;
                }
            }
            else if (inRegex) {
                if (c === 0x2f && prev !== 0x5C) {
                    inRegex = false;
                }
            }
            else if (c === 0x7C && // pipe
                exp.charCodeAt(i + 1) !== 0x7C &&
                exp.charCodeAt(i - 1) !== 0x7C &&
                !curly && !square && !paren) {
                if (expression === undefined) {
                    // first filter, end of expression
                    lastFilterIndex = i + 1;
                    expression = exp.slice(0, i).trim();
                }
                else {
                    pushFilter();
                }
            }
            else {
                switch (c) {
                    case 0x22:
                        inDouble = true;
                        break; // "
                    case 0x27:
                        inSingle = true;
                        break; // '
                    case 0x60:
                        inTemplateString = true;
                        break; // `
                    case 0x28:
                        paren++;
                        break; // (
                    case 0x29:
                        paren--;
                        break; // )
                    case 0x5B:
                        square++;
                        break; // [
                    case 0x5D:
                        square--;
                        break; // ]
                    case 0x7B:
                        curly++;
                        break; // {
                    case 0x7D:
                        curly--;
                        break; // }
                }
                if (c === 0x2f) { // /
                    var j = i - 1;
                    var p = (void 0);
                    // find first non-whitespace prev char
                    for (; j >= 0; j--) {
                        p = exp.charAt(j);
                        if (p !== ' ') {
                            break;
                        }
                    }
                    if (!p || !validDivisionCharRE.test(p)) {
                        inRegex = true;
                    }
                }
            }
        }
        if (expression === undefined) {
            expression = exp.slice(0, i).trim();
        }
        else if (lastFilterIndex !== 0) {
            pushFilter();
        }
        function pushFilter() {
            (filters || (filters = [])).push(exp.slice(lastFilterIndex, i).trim());
            lastFilterIndex = i + 1;
        }
        if (filters) {
            for (i = 0; i < filters.length; i++) {
                expression = wrapFilter(expression, filters[i]);
            }
        }
        return expression;
    }
    function wrapFilter(exp, filter) {
        var i = filter.indexOf('(');
        if (i < 0) {
            // _f: resolveFilter
            return ("_f(\"" + filter + "\")(" + exp + ")");
        }
        else {
            var name = filter.slice(0, i);
            var args = filter.slice(i + 1);
            return ("_f(\"" + name + "\")(" + exp + (args !== ')' ? ',' + args : args));
        }
    }
    var defaultTagRE = /\{\{((?:.|\r?\n)+?)\}\}/g;
    var regexEscapeRE = /[-.*+?^${}()|[\]\/\\]/g;
    /**
     * Create a cached version of a pure function.
     */
    function cached(fn) {
        var cache = Object.create(null);
        return (function cachedFn(str) {
            var hit = cache[str];
            return hit || (cache[str] = fn(str));
        });
    }
    var buildRegex = cached(function (delimiters) {
        var open = delimiters[0].replace(regexEscapeRE, '\\$&');
        var close = delimiters[1].replace(regexEscapeRE, '\\$&');
        return new RegExp(open + '((?:.|\\n)+?)' + close, 'g');
    });
    function parseText(text, delimiters) {
        var tagRE = delimiters ? buildRegex(delimiters) : defaultTagRE;
        if (!tagRE.test(text)) {
            console.error(tagRE, text);
            return;
        }
        var tokens = [];
        var rawTokens = [];
        var lastIndex = tagRE.lastIndex = 0;
        var match, index, tokenValue;
        while ((match = tagRE.exec(text))) {
            index = match.index;
            // push text token
            if (index > lastIndex) {
                rawTokens.push(tokenValue = text.slice(lastIndex, index));
                tokens.push(JSON.stringify(tokenValue));
            }
            // tag token
            var exp = parseFilters(match[1].trim());
            tokens.push(("_s(" + exp + ")"));
            rawTokens.push({ '@binding': exp });
            lastIndex = index + match[0].length;
        }
        if (lastIndex < text.length) {
            rawTokens.push(tokenValue = text.slice(lastIndex));
            tokens.push(JSON.stringify(tokenValue));
        }
        return {
            expression: tokens.join('+'),
            tokens: rawTokens
        };
    }
    class ViewModel {
        static create(data) {
            let vm = new ViewModel(data);
            return vm;
        }
        constructor(data) {
            Object.defineProperty(this, "_watchers", {
                value: [],
                enumerable: false,
                writable: true,
                configurable: false,
            });
            Object.defineProperty(this, "_isVue", {
                value: true,
                enumerable: false,
                writable: false,
                configurable: false,
            });
            let keys = Object.keys(data);
            let i = keys.length;
            while (i--) {
                let key = keys[i];
                Object.defineProperty(this, key, {
                    enumerable: true,
                    configurable: true,
                    get: function () {
                        return data[key];
                    },
                    set: function (val) {
                        data[key] = val;
                    },
                });
            }
            observe(data, true);
        }
        static $toString(v) {
            return new String(v);
        }
        // 临时代码
        static flush() {
            flushSchedulerQueue();
        }
        static expression(vm, expression, cb) {
            if (vm instanceof ViewModel) {
                let exp = parseText(expression, null);
                let fn = function () { return eval("(function (_s) { return " + exp.expression + "; })").call(this, ViewModel.$toString); };
                return new Watcher(vm, fn, cb);
            }
        }
        static field(vm, path, cb) {
            if (vm instanceof ViewModel) {
                return new Watcher(vm, path, cb);
            }
        }
    }
    exports.ViewModel = ViewModel;
});
define("plover/jsx/element", ["require", "exports", "UnityEngine.UI", "plover/jsx/vue"], function (require, exports, UnityEngine_UI_1, vue_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.JSXText = exports.JSXWidget = exports.JSXCompoundNode = exports.registerElement = exports.createElement = exports.element = exports.findUIComponent = exports.JSXNode = void 0;
    let elementActivators = {};
    class JSXNode {
        get parent() { return this._parent; }
        set parent(value) {
            if (this._parent != value) {
                this._parent = value;
                this.onParentSet();
            }
        }
        get widget() {
            let p = this._parent;
            while (p) {
                if (p instanceof JSXWidget) {
                    return p;
                }
                p = p._parent;
            }
        }
    }
    exports.JSXNode = JSXNode;
    function findUIComponent(transform, name, type) {
        let n = transform.childCount;
        for (let i = 0; i < n; i++) {
            let child = transform.GetChild(i);
            if (child.name == name) {
                let com = child.GetComponent(type);
                if (com) {
                    return com;
                }
            }
            let com = findUIComponent(child, name, type);
            if (com) {
                return com;
            }
        }
    }
    exports.findUIComponent = findUIComponent;
    function element(name) {
        return function (target) {
            registerElement(name, target);
        };
    }
    exports.element = element;
    function createElement(name, attributes, ...children) {
        let act = elementActivators[name];
        if (typeof act !== "undefined") {
            let element = new act();
            element.init(attributes, ...children);
            return element;
        }
    }
    exports.createElement = createElement;
    function registerElement(name, activator) {
        elementActivators[name] = activator;
    }
    exports.registerElement = registerElement;
    class JSXCompoundNode extends JSXNode {
        init(attributes, ...children) {
            this._children = children;
            for (let i = 0; i < this._children.length; i++) {
                let child = this._children[i];
                child.parent = this;
            }
        }
        evaluate() {
            for (let i = 0; i < this._children.length; i++) {
                let child = this._children[i];
                child.evaluate();
            }
        }
        destroy() {
            for (let i = 0; i < this._children.length; i++) {
                let child = this._children[i];
                child.destroy();
            }
        }
    }
    exports.JSXCompoundNode = JSXCompoundNode;
    // export interface IWidgetInstance {
    //     readonly gameObject: GameObject;
    //     readonly data: any;
    // }
    let JSXWidget = class JSXWidget extends JSXCompoundNode {
        get instance() { return this._instance; }
        get data() { return this._instance.data; }
        init(attributes, ...children) {
            this._instance = attributes.class;
            super.init(attributes, ...children);
        }
        onParentSet() {
        }
    };
    JSXWidget = __decorate([
        element("widget")
    ], JSXWidget);
    exports.JSXWidget = JSXWidget;
    let JSXText = class JSXText extends JSXNode {
        init(attributes, ...children) {
            if (attributes) {
                this._name = attributes.name;
                this._text = attributes.text;
            }
        }
        onParentSet() {
            this._component = findUIComponent(this.widget.instance.transform, this._name, UnityEngine_UI_1.Text);
            this._watcher = vue_1.ViewModel.expression(this.widget.data, this._text, this.onValueChanged.bind(this));
        }
        onValueChanged(value) {
            this._component.text = value;
        }
        evaluate() {
            if (this._watcher) {
                this._watcher.evaluate();
            }
        }
        destroy() {
            if (this._watcher) {
                this._watcher.teardown();
                this._watcher = null;
            }
        }
    };
    JSXText = __decorate([
        element("text")
    ], JSXText);
    exports.JSXText = JSXText;
});
define("plover/jsx/bridge", ["require", "exports", "UnityEngine", "plover/runtime/class_decorators"], function (require, exports, UnityEngine_15, class_decorators_2) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.JSXWidgetBridge = void 0;
    let JSXWidgetBridge = class JSXWidgetBridge extends UnityEngine_15.MonoBehaviour {
        get data() { return null; }
        OnDestroy() {
            if (this._widget) {
                this._widget.destroy();
            }
        }
    };
    JSXWidgetBridge = __decorate([
        class_decorators_2.ScriptType()
    ], JSXWidgetBridge);
    exports.JSXWidgetBridge = JSXWidgetBridge;
});
// // not implemented yet, it's imagination for fun
// console.log("test jsx");
// let userWidget = <widget>
//     <label name="test" bind="expression {test.value}" />
//     <list name="list_test" bind="mydata.mylist" entry-class="SomeType" />
//     <button name="button_test" bind="mydata.myaction" onclick="this.onclick" />
// </widget>
//# sourceMappingURL=plover.js.map