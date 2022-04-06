/// <reference types="node" />
declare module "plover/events/dispatcher" {
    export class Handler {
        caller: any;
        fn: Function;
        once: boolean;
        constructor(caller: any, fn: Function, once?: boolean);
        invoke(arg0?: any, arg1?: any, arg2?: any): void;
    }
    /**
     * 简单的事件分发器实现
     * 此实现功能与 DuktapeJS.Dispatcher 基本一致,
     * 但 DuktapeJS.Dispatcher 不保证事件响应顺序, 但效率更高 (因为复用了中途移除的索引)
     */
    export class Dispatcher {
        private _handlers;
        on(caller: any, fn: Function): Handler;
        once(caller: any, fn: Function): Handler;
        off(caller: any, fn: Function): boolean;
        /**
         * 移除所有处理器
         */
        clear(): void;
        dispatch(arg0?: any, arg1?: any, arg2?: any): void;
    }
    /**
     * 按事件名派发
     */
    export class EventDispatcher {
        private _dispatcher;
        on(evt: string, caller: any, fn?: Function): void;
        once(evt: string, caller: any, fn?: Function): void;
        off(evt: string, caller: any, fn?: Function): void;
        clear(): void;
        /**
         * 派发指定事件
         */
        dispatch(evt: string, arg0?: any, arg1?: any, arg2?: any): void;
    }
}
declare module "plover/editor/auto_completion_field" {
    import { EventDispatcher } from "plover/events/dispatcher";
    export class AutoCompletionField extends EventDispatcher {
        searchString: string;
        maxResults: number;
        private results;
        private selectedIndex;
        private searchField;
        private previousMousePosition;
        private selectedIndexByMouse;
        private showResults;
        constructor();
        addResult(result: string): void;
        clearResults(): void;
        onToolbarGUI(): void;
        onGUI(): void;
        private draw;
        private doSearchField;
        private onDownOrUpArrowKeyPressed;
        private doResults;
        private onConfirm;
        private hasSearchbarFocused;
        private repaintFocusedWindow;
    }
}
declare module "plover/runtime/serialize" {
    import { ByteBuffer } from "QuickJS.IO";
    import { JSSerializationContext } from "QuickJS.Unity";
    export interface IPrimitiveSerializer {
        typeid: number;
        defaultValue: any;
        serialize(context: SerializationContext, buffer: ByteBuffer, value: any): void;
        deserilize(context: SerializationContext, buffer: ByteBuffer): any;
    }
    export interface PrimitiveSerializerImpl {
        dataFormat: number;
        description: string;
        types: {
            [key: string]: IPrimitiveSerializer;
        };
        typeids: IPrimitiveSerializer[];
    }
    export interface SerializationContext {
        impl: PrimitiveSerializerImpl;
        ps: JSSerializationContext;
    }
    export enum SerializedTypeID {
        Null = 0,
        UserDefinedMin = 1,
        UserDefinedMax = 100,
        Array = 101,
        Object = 102
    }
    export function GetLatestSerializer(): PrimitiveSerializerImpl;
    export function GetSerializer(dataFormat: number): PrimitiveSerializerImpl;
    export function RegisterSerializer(dataFormat: number, description: string, types: {
        [key: string]: IPrimitiveSerializer;
    }, bSetAsLatest?: boolean): void;
}
declare module "plover/runtime/class_decorators" {
    import { ByteBuffer } from "QuickJS.IO";
    import { JSSerializationContext } from "QuickJS.Unity";
    export interface FunctionMetaInfo {
    }
    export interface ClassMetaInfo {
    }
    export type PropertyTypeID = "bool" | "float" | "double" | "string" | "object" | "int" | "uint" | "Uint8ArrayBuffer" | "Vector2" | "Vector3" | "Vector4" | "Rect" | "Quaternion" | "json" | Function;
    export type PropertyLayout = "plain" | "array";
    export interface WeakPropertyMetaInfo {
        /**
         * slot name in property table
         */
        name?: string;
        propertyKey?: string;
        /**
         * (默认编辑器行为中) 是否可见
         */
        visible?: boolean;
        /**
         * (默认编辑器行为中) 是否可以编辑
         */
        editable?: boolean;
        /**
         * 是否仅编辑器状态可编辑
         */
        editorOnly?: boolean;
        /**
         * 是否序列化
         */
        serializable?: boolean;
        label?: string;
        tooltip?: string;
        extra?: any;
        /**
         * UGUI, 自动绑定界面组件
         */
        bind?: {
            name?: string;
            widget?: Function;
        };
    }
    export interface PropertyMetaInfo extends WeakPropertyMetaInfo {
        type: PropertyTypeID;
        layout?: PropertyLayout;
    }
    export function ScriptSerializable(meta?: any): (target: any) => any;
    export function ScriptAsset(meta?: any): (target: any) => any;
    export function ScriptType(meta?: ClassMetaInfo): (target: any) => any;
    export function ScriptInteger(meta?: WeakPropertyMetaInfo): (target: any, propertyKey: string) => void;
    export function ScriptNumber(meta?: WeakPropertyMetaInfo): (target: any, propertyKey: string) => void;
    export function ScriptString(meta?: WeakPropertyMetaInfo): (target: any, propertyKey: string) => void;
    export function ScriptObject(meta?: WeakPropertyMetaInfo): (target: any, propertyKey: string) => void;
    export function ScriptProperty(meta?: PropertyMetaInfo): (target: any, propertyKey: string) => void;
    export function ScriptFunction(meta?: any): (target: any, propertyKey: string) => void;
    export class SerializationUtil {
        static forEach(target: any, cb: (slots: {
            [key: string]: PropertyMetaInfo;
        }, propertyKey: string) => void): void;
        static markAsReady(target: any): void;
        static serialize(target: any, ps: JSSerializationContext): void;
        private static _serializeValue;
        private static _serializeObjectArray;
        private static _serializePrimitiveArray;
        private static _serializeObject;
        static deserialize(target: any, ps: JSSerializationContext, buffer: ByteBuffer): void;
        private static _deserializeObject;
        private static _deserializeObjectArray;
        private static _deserializePrimitiveArray;
        private static _deserializeObjectInternal;
    }
}
declare module "plover/editor/drawer" {
    import { PropertyMetaInfo } from "plover/runtime/class_decorators";
    interface IPropertyDrawer {
        draw(value: any, prop: PropertyMetaInfo, label: string, editablePE: boolean): any;
    }
    export let DefaultPropertyDrawers: {
        [key: string]: IPropertyDrawer;
    };
}
declare module "plover/editor/editor_decorators" {
    import { Editor } from "UnityEditor";
    import { ClassMetaInfo } from "plover/runtime/class_decorators";
    export interface EditorWindowMetaInfo extends ClassMetaInfo {
    }
    export function ScriptEditor(forType: any): (editorType: any) => any;
    export function ScriptEditorWindow(meta?: EditorWindowMetaInfo): (target: any) => any;
    export interface IEditorScriptingSupport {
        OnPropertyChanging(target: any, property: any, propertyKey: string, newValue: any): void;
        OnArrayPropertyChanging(target: any, property: any, propertyKey: string, index: number, newValue: any): void;
    }
    export class DefaultEditor extends Editor implements IEditorScriptingSupport {
        OnPropertyPreChanging(target: any, name: string): void;
        OnPropertyChanging(target: any, property: any, propertyKey: string, newValue: any): void;
        OnArrayPropertyChanging(target: any, property: any, propertyKey: string, index: number, newValue: any): void;
        OnInspectorGUI(): void;
    }
    export class EditorUtil {
        static getCustomEditor(forType: any): any;
        /**
         * 默认编辑器绘制行为
         */
        static draw(editor: IEditorScriptingSupport, target: any): void;
    }
}
declare module "plover/editor/file_watcher" {
    export enum EFileState {
        CHANGE = 1,
        NEW = 2,
        DELETE = 3
    }
    export interface FileState {
        name: string;
        fullPath: string;
        state: EFileState;
    }
    export interface IFileStateMap {
        [name: string]: FileState;
    }
    export class FileWatcher {
        static readonly ANY = "* ANY";
        static readonly CHANGED = "* CHANGED";
        private _fsw;
        private _dispatcher;
        private _disposed;
        private _pending;
        private _cache;
        get includeSubdirectories(): boolean;
        set includeSubdirectories(v: boolean);
        get enableRaisingEvents(): boolean;
        set enableRaisingEvents(v: boolean);
        constructor(path: string, filter: string);
        dispose(): void;
        on(name: string, caller: any, fn: Function): void;
        off(name: string, caller: any, fn: Function): void;
        private oncreate;
        private onchange;
        private ondelete;
        private setCacheState;
        private dispatchEvents;
    }
}
declare module "plover/editor/js_console" {
    import { EditorWindow } from "UnityEditor";
    export function fillAutoCompletion(scope: any, pattern: string): Array<string>;
    export class JSConsole extends EditorWindow {
        private _searchField;
        private _history;
        Awake(): void;
        private onSearchChange;
        private onSearchConfirm;
        OnEnable(): void;
        OnGUI(): void;
    }
}
declare module "plover/text/string_utils" {
    export let DefaultMatcher: RegExp;
    export function TEXT(s: string): string;
    /**
     * 按一个上下文对象替换字符串中相应的关键字匹配串
     */
    export class Contextual {
        private re;
        private context;
        constructor(re?: RegExp);
        /**
         * 绑定一个上下文对象 （通常是一个map）
         */
        bind(context: any): this;
        /**
         * 替换字符串匹配串 (可以传入上下文，否则使用绑定的)
         * context 可以是一个支持嵌套数据的map, 也可以是一个处理函数.
         * 当使用处理函数时, 参数为取值key字符串.
         */
        replace(text: string, context?: any): string;
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
        static replace(text: string, context: any, re?: RegExp): string;
    }
    export class StringUtil {
        /**
         * 替换字符串中的字串
         * @param oldString 原字符串
         * @param matchString 匹配字串
         * @param replaceString 替换字串
         * @returns 替换后的字符串
         */
        static replaceAll(oldString: string, matchString: string, replaceString: string): string;
        static contains(str: string, match: string): boolean;
        /**
         * 数字 => 前缀0固定长度字符串
         */
        static prefix(num: number | string, length: number): string;
        /**
         * 转换为固定小数和整数部分长度的字符串
         */
        static prefix2(num: number, length1: number, length2: number): string;
        /**
         * 简单字符串表示的时长 (mm:ss.mmm)
         */
        static time(deltaTime: number): string;
    }
}
declare module "plover/editor/base/menu_builder" {
    import { GenericMenu } from "UnityEditor";
    export interface IMenuItem {
        name: string;
        build(menu: GenericMenu): any;
    }
    export abstract class MenuAbstractItem implements IMenuItem {
        private _name;
        get name(): string;
        constructor(name: string);
        abstract build(menu: GenericMenu): any;
    }
    export class MenuSeparator extends MenuAbstractItem {
        build(menu: GenericMenu): void;
    }
    export class MenuAction extends MenuAbstractItem {
        private _action;
        get action(): Function;
        constructor(name: string, action: Function);
        build(menu: GenericMenu): void;
    }
    export class MenuDisabledAction extends MenuAbstractItem {
        constructor(name: string);
        build(menu: GenericMenu): void;
    }
    export class MenuBuilder {
        private _items;
        addAction(name: string, action: Function, isDisabled?: boolean): void;
        addDisabledAction(name: string): void;
        addSeperator(): void;
        build(): GenericMenu;
    }
}
declare module "plover/editor/base/splitview" {
    import { EditorWindow } from "UnityEditor";
    import { Color, Rect } from "UnityEngine";
    export class HSplitView {
        init: boolean;
        splitPivot: number;
        resize: boolean;
        cursorChangeRect: Rect;
        cursorHintRect: Rect;
        cursorHintSize: number;
        cursorSize: number;
        cursorHintColor: Color;
        draw(window: EditorWindow, startY: number, fullWidth: number, fullHeight: number): void;
    }
}
declare module "plover/editor/base/treeview" {
    import { ITreeNodeEventHandler, UTreeNode } from "plover/editor/base/treenode";
    export class UTreeView {
        static readonly CONTEXT_MENU = "CONTEXT_MENU";
        readonly SKIP_RETURN = 0;
        private _handler;
        private _events;
        private _eventUsed;
        private _skipReturn;
        private _root;
        private _height;
        private _drawY;
        private _rowIndex;
        private _indentSize;
        private _controlRect;
        private _controlID;
        private _controlEventType;
        private _controlMousePos;
        private _rowRect;
        private _indentRect;
        private _tempRect;
        private _selected;
        private _editing;
        private _deferredMenuPopup;
        private _searchString;
        private _selectionColor;
        private _rowColor;
        private _focusColor;
        private _debug_touchChild;
        private _debug_drawChild;
        get selected(): UTreeNode;
        set selected(value: UTreeNode);
        get searchString(): string;
        set searchString(value: string);
        get root(): UTreeNode;
        get handler(): ITreeNodeEventHandler;
        set handler(value: ITreeNodeEventHandler);
        constructor(handler: ITreeNodeEventHandler);
        on(evt: string, caller: any, fn?: Function): void;
        off(evt: string, caller: any, fn?: Function): void;
        dispatch(name: string, arg0?: any, arg1?: any, arg2?: any): void;
        allocFolderHierarchy(path: string, data: any): UTreeNode;
        getFolderHierarchy(path: string): UTreeNode;
        private _getFolderHierarchy;
        removeAll(): void;
        deleteNode(node: UTreeNode): boolean;
        search(p: string): void;
        private _search;
        expandAll(): void;
        collapseAll(): void;
        draw(offsetX: number, offsetY: number, width: number, height: number): boolean;
        private calcRowHeight;
        private calcSearchResultsHeight;
        private setControlRect;
        private useEvent;
        private drawSearchResults;
        private drawRow;
        findPreviousNode(node: UTreeNode): UTreeNode;
        findNextNode(node: UTreeNode): UTreeNode;
    }
}
declare module "plover/editor/base/treenode" {
    import { GUIContent, Rect, Texture, Vector2 } from "UnityEngine";
    import { EventDispatcher } from "plover/events/dispatcher";
    import { MenuBuilder } from "plover/editor/base/menu_builder";
    import { UTreeView } from "plover/editor/base/treeview";
    export interface ITreeNodeEventHandler {
        onTreeNodeContextMenu(node: UTreeNode, builder: MenuBuilder): any;
        onTreeNodeCreated(node: UTreeNode): any;
        onTreeNodeNameEditEnded(node: UTreeNode, newName: string): any;
        onTreeNodeNameChanged(node: UTreeNode, oldName: string): any;
    }
    export class BuiltinIcons {
        private static _cache;
        static getIcon(name: string): Texture;
    }
    export class UTreeNode {
        protected _tree: UTreeView;
        protected _parent: UTreeNode;
        protected _children: Array<UTreeNode>;
        protected _expanded: boolean;
        protected _name: string;
        protected _events: EventDispatcher;
        data: any;
        isSearchable: boolean;
        isEditable: boolean;
        _foldoutRect: Rect;
        protected _label: GUIContent;
        protected _folderClose: Texture;
        protected _folderOpen: Texture;
        private _bFocusTextField;
        private _bVisible;
        private _height;
        private _bMatch;
        get isMatch(): boolean;
        get height(): number;
        /**
         * 当前层级是否展开
         */
        get expanded(): boolean;
        set expanded(value: boolean);
        get isFolder(): boolean;
        get visible(): boolean;
        set visible(value: boolean);
        get parent(): UTreeNode;
        get isRoot(): boolean;
        get name(): string;
        set name(value: string);
        get fullPath(): string;
        get treeView(): UTreeView;
        constructor(tree: UTreeView, parent: UTreeNode, isFolder: boolean, name: string);
        get childCount(): number;
        on(evt: string, caller: any, fn?: Function): void;
        off(evt: string, caller: any, fn?: Function): void;
        dispatch(name: string, arg0?: any, arg1?: any, arg2?: any): void;
        match(p: string): boolean;
        getRelativePath(top: UTreeNode): string;
        expandAll(): void;
        collapseAll(): void;
        private _setExpandAll;
        expandUp(): void;
        /**
         * 获取指定节点的在当前层级中的下一个相邻节点
         */
        findNextSibling(node: UTreeNode): UTreeNode;
        /**
         * 获取指定节点的在当前层级中的上一个相邻节点
         */
        findLastSibling(node: UTreeNode): UTreeNode;
        forEachChild(fn: (child: UTreeNode) => void): void;
        /**
         * 获取当前层级下的子节点
         * @param index 索引 或者 命名
         * @param autoNew 不存在时是否创建 (仅通过命名获取时有效)
         * @returns 子节点
         */
        getFolderByName(name: string, isAutoCreate: boolean, data: any): UTreeNode;
        getLeafByName(name: string, isAutoCreate: boolean, data: any): UTreeNode;
        getChildByIndex(index: number): UTreeNode;
        /**
         * 当前层级最后一个子节点
         */
        getLastChild(): UTreeNode;
        /**
         * 当前层级第一个子节点
         */
        getFirstChild(): UTreeNode;
        addFolderChild(name: string): UTreeNode;
        addLeafChild(name: string): UTreeNode;
        allocLeafChild(name: string, data: any): UTreeNode;
        /**
         * 在当前层级添加一个子节点
         */
        private _addChild;
        /**
         * 将一个子节点从当前层级中移除
         */
        removeChild(node: UTreeNode): boolean;
        removeAll(): void;
        calcRowHeight(): number;
        drawMenu(treeView: UTreeView, pos: Vector2, handler: ITreeNodeEventHandler): void;
        draw(rect: Rect, bSelected: boolean, bEditing: boolean, indentSize: number): void;
        endEdit(): void;
    }
}
declare module "plover/editor/base/breadcrumb" {
    import { UTreeNode } from "plover/editor/base/treenode";
    import { EventDispatcher } from "plover/events/dispatcher";
    export class Breadcrumb extends EventDispatcher {
        static readonly CLICKED = "CLICKED";
        private _height;
        private _heightOptionSV;
        private _heightOptionHB;
        private _cache;
        private _color;
        private _sv;
        get height(): number;
        constructor();
        draw(node: UTreeNode): void;
    }
}
declare module "plover/editor/base/editor_window_base" {
    import { EditorWindow } from "UnityEditor";
    import { Event, GUIContent, Rect, Vector2 } from "UnityEngine";
    import { MenuBuilder } from "plover/editor/base/menu_builder";
    import { HSplitView } from "plover/editor/base/splitview";
    import { ITreeNodeEventHandler, UTreeNode } from "plover/editor/base/treenode";
    import { UTreeView } from "plover/editor/base/treeview";
    import { Breadcrumb } from "plover/editor/base/breadcrumb";
    export abstract class EditorWindowBase extends EditorWindow implements ITreeNodeEventHandler {
        protected _treeView: UTreeView;
        protected _breadcrumb: Breadcrumb;
        protected _treeViewScroll: Vector2;
        protected _hSplitView: HSplitView;
        protected _toolbarRect: Rect;
        protected _leftRect: Rect;
        protected _rightRect: Rect;
        protected _searchLabel: GUIContent;
        protected _tempRect: Rect;
        protected _event: Event;
        protected _contents: {
            [key: string]: GUIContent;
        };
        toobarHeight: number;
        onTreeNodeNameEditEnded(node: UTreeNode, newName: string): void;
        onTreeNodeNameChanged(node: UTreeNode, oldName: string): void;
        onTreeNodeCreated(node: UTreeNode): void;
        onTreeNodeContextMenu(node: UTreeNode, builder: MenuBuilder): void;
        buildBreadcrumbMenu(top: UTreeNode, node: UTreeNode, builder: MenuBuilder): void;
        onClickBreadcrumb(node: UTreeNode, isContext: boolean): void;
        Awake(): void;
        private drawLeftTreeView;
        protected drawConfigView(data: any, node: UTreeNode): void;
        protected drawFolderView(data: any, node: UTreeNode): void;
        protected abstract drawToolBar(): any;
        protected TRect(x: number, y: number, w: number, h: number): Rect;
        protected TContent(name: string, icon: string, tooltip?: string, text?: string): GUIContent;
        OnGUI(): void;
    }
}
declare module "plover/editor/js_reload" {
    export function reload(mod: NodeModule): void;
}
declare module "plover/editor/js_module_view" {
    import { EditorWindowBase } from "plover/editor/base/editor_window_base";
    import { UTreeNode } from "plover/editor/base/treenode";
    export class JSModuleView extends EditorWindowBase {
        private _touch;
        Awake(): void;
        OnEnable(): void;
        protected drawFolderView(data: any, node: UTreeNode): void;
        protected drawToolBar(): void;
        private updateModules;
        private getSimplifiedName;
        private addModule;
    }
}
declare module "plover/editor/base/content_cache" {
    import { GUIContent, Texture } from "UnityEngine";
    export class EdCache {
        static cache: {
            [key: string]: GUIContent;
        };
        static T(title: string, tooltip?: string, image?: Texture): GUIContent;
    }
}
declare module "plover/events/data_binding" {
    export abstract class Subscriber {
        private _model;
        private _key;
        private _source;
        constructor(model: DataBinding, key: string);
        get value(): any;
        set value(newValue: any);
        protected update(value: any): void;
        notify(value: any): void;
        unsubscribe(): void;
    }
    export class Subscribers {
        private _subs;
        notify(valueProxy: any): void;
        addSub(sub: Subscriber): void;
        removeSub(sub: Subscriber): void;
        transfer(newValue: Subscribers): void;
    }
    export class DataBinding {
        private constructor();
        addSubscriber(sub: Subscriber): void;
        removeSubscriber(sub: Subscriber): void;
        static bind<T>(data: T): T;
        static subscribe<T extends Subscriber>(SubscriberType: {
            new (model: DataBinding, key: string, ...args: any[]): T;
        }, modelObject: any, path: string, ...args: any[]): T;
    }
}
declare module "plover/jsx/vue" {
    export interface IViewModelWatcher {
        readonly value: string;
        readonly dirty: boolean;
        readonly expression: string;
        readonly id: number;
        cb: (value: any, oldValue: any) => void;
        evaluate: () => void;
        teardown: () => void;
    }
    export class ViewModel {
        static create<T>(data: T): T;
        constructor(data: any);
        static $toString(v: any): String;
        static flush(): void;
        static expression(vm: any, expression: string, cb?: (value: any, oldValue: any) => void): IViewModelWatcher;
        static field(vm: any, path: string, cb?: (value: any, oldValue: any) => void): IViewModelWatcher;
    }
}
declare module "plover/jsx/element" {
    import { Component, Transform } from "UnityEngine";
    import { JSXWidgetBridge } from "plover/jsx/bridge";
    export interface Activator<T = JSXNode> {
        new (): T;
    }
    export abstract class JSXNode {
        private _parent;
        get parent(): JSXNode;
        set parent(value: JSXNode);
        get widget(): JSXWidget;
        protected abstract onParentSet(): any;
        abstract init(attributes: any, ...children: Array<JSXNode>): any;
        abstract evaluate(): any;
        abstract destroy(): any;
    }
    export function findUIComponent<T extends Component>(transform: Transform, name: string, type: {
        new (): T;
    }): T;
    export function element(name: string): (target: any) => void;
    export function createElement(name: string, attributes: any, ...children: Array<JSXNode>): JSXNode;
    export function registerElement(name: string, activator: Activator): void;
    export abstract class JSXCompoundNode extends JSXNode {
        private _children;
        init(attributes: any, ...children: Array<JSXNode>): void;
        evaluate(): void;
        destroy(): void;
    }
    export class JSXWidget extends JSXCompoundNode {
        private _instance;
        get instance(): JSXWidgetBridge;
        get data(): any;
        init(attributes: any, ...children: Array<JSXNode>): void;
        protected onParentSet(): void;
    }
    export class JSXText extends JSXNode {
        private _name;
        private _text;
        private _component;
        private _watcher;
        init(attributes: any, ...children: Array<JSXNode>): void;
        protected onParentSet(): void;
        private onValueChanged;
        evaluate(): void;
        destroy(): void;
    }
}
declare module "plover/jsx/bridge" {
    import { MonoBehaviour } from "UnityEngine";
    import * as JSX from "plover/jsx/element";
    export abstract class JSXWidgetBridge extends MonoBehaviour {
        protected _widget: JSX.JSXWidget;
        get data(): any;
        OnDestroy(): void;
    }
}
