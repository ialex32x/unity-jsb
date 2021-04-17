#if UNITY_EDITOR
namespace QuickJS.Unity
{
    using UnityEditor;
    using Native;

    [CustomEditor(typeof(JSBehaviour))]
    public class JSBehaviourInspector : Editor
    {
        private bool _released;
        private JSContext _ctx;
        private JSValue _this_obj;

        private bool _onDestroyValid;
        private JSValue _onDestroyFunc;

        private bool _onEnableValid;
        private JSValue _onEnableFunc;

        private bool _onDisableValid;
        private JSValue _onDisableFunc;

        private bool _onInspectorGUIValid;
        private JSValue _onInspectorGUIFunc;

        public void SetBridge(JSContext ctx, JSValue this_obj, JSValue ctor)
        {
            var context = ScriptEngine.GetContext(ctx);
            if (context == null)
            {
                return;
            }

            context.OnDestroy += OnContextDestroy;
            _released = false;
            _ctx = ctx;
            _this_obj = JSApi.JS_DupValue(ctx, this_obj);

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            _onEnableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnEnable"));
            _onEnableValid = JSApi.JS_IsFunction(ctx, _onEnableFunc) == 1;

            _onDisableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDisable"));
            _onDisableValid = JSApi.JS_IsFunction(ctx, _onDisableFunc) == 1;

            _onInspectorGUIFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnInspectorGUI"));
            _onInspectorGUIValid = JSApi.JS_IsFunction(ctx, _onInspectorGUIFunc) == 1;

            var awake_obj = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));

            Call(awake_obj);
            JSApi.JS_FreeValue(_ctx, awake_obj);
        }

        private void Call(JSValue func_obj)
        {
            if (JSApi.JS_IsFunction(_ctx, func_obj) == 1)
            {
                var rval = JSApi.JS_Call(_ctx, func_obj, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        private void OnContextDestroy(ScriptContext context)
        {
            Release();
        }

        void Release()
        {
            if (_released)
            {
                return;
            }
            _released = true;

            JSApi.JS_FreeValue(_ctx, _onDestroyFunc);
            _onDestroyValid = false;

            JSApi.JS_FreeValue(_ctx, _onEnableFunc);
            _onEnableValid = false;

            JSApi.JS_FreeValue(_ctx, _onDisableFunc);
            _onDisableValid = false;

            JSApi.JS_FreeValue(_ctx, _onInspectorGUIFunc);
            _onInspectorGUIValid = false;

            JSApi.JS_FreeValue(_ctx, _this_obj);

            var context = ScriptEngine.GetContext(_ctx);
            if (context != null)
            {
                context.OnDestroy -= OnContextDestroy;
            }
        }

        private void SetBridge()
        {
            var inst = target as JSBehaviour;
            var ctx = inst.ctx;
            if (ctx.IsValid())
            {
                var editorClass = inst.GetProperty("__editor__");
                if (JSApi.JS_IsConstructor(ctx, editorClass) == 1)
                {
                    var cache = ScriptEngine.GetObjectCache(ctx);
                    var object_id = cache.AddObject(this, false);
                    var val = JSApi.jsb_construct_bridge_object(ctx, editorClass, object_id);
                    if (val.IsException())
                    {
                        ctx.print_exception();
                        cache.RemoveObject(object_id);
                    }
                    else
                    {
                        cache.AddJSValue(this, val);
                        this.SetBridge(ctx, val, editorClass);
                        JSApi.JS_FreeValue(ctx, val);
                    }
                }
                JSApi.JS_FreeValue(ctx, editorClass);
            }
        }

        void Awake()
        {
            SetBridge();
        }

        void OnEnable()
        {
            if (_onEnableValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onEnableFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void OnDisable()
        {
            if (_onDisableValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDisableFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void OnDestroy()
        {
            if (_onDestroyValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDestroyFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
            Release();
        }

        public override void OnInspectorGUI()
        {
            if (UnityEditor.EditorApplication.isCompiling)
            {
                Release();
                EditorGUILayout.HelpBox("Temporarily unavailable", MessageType.Warning);
                return;
            }

            if (_onInspectorGUIValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onInspectorGUIFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
            // else
            // {
            //     var inst = target as JSBehaviour;

            //     EditorGUILayout.TextField("Script Type", inst.scriptTypeName);
            //     inst.ForEachProperty((ctx, atom, prop) =>
            //     {
            //         var strValue = JSApi.JS_AtomToString(ctx, atom);
            //         var str = JSApi.GetString(ctx, strValue);
            //         JSApi.JS_FreeValue(ctx, strValue);

            //         switch (prop.tag)
            //         {
            //             case JSApi.JS_TAG_BOOL:
            //                 EditorGUILayout.Toggle(str, JSApi.JS_ToBool(ctx, prop) == 1);
            //                 break;
            //             case JSApi.JS_TAG_STRING:
            //                 {
            //                     var pres = JSApi.GetString(ctx, prop);
            //                     EditorGUILayout.TextField(str, pres);
            //                 }
            //                 break;
            //             case JSApi.JS_TAG_FLOAT64:
            //                 {
            //                     double pres;
            //                     if (JSApi.JS_ToFloat64(ctx, out pres, prop) == 0)
            //                     {
            //                         EditorGUILayout.FloatField(str, (float)pres);
            //                     }
            //                     else
            //                     {
            //                         EditorGUILayout.TextField(str, "[ParseFailed]");
            //                     }
            //                 }
            //                 break;
            //             case JSApi.JS_TAG_INT:
            //                 {
            //                     int pres;
            //                     if (JSApi.JS_ToInt32(ctx, out pres, prop) == 0)
            //                     {
            //                         EditorGUILayout.IntField(str, pres);
            //                     }
            //                     else
            //                     {
            //                         EditorGUILayout.TextField(str, "[ParseFailed]");
            //                     }
            //                 }
            //                 break;
            //             default:
            //                 EditorGUILayout.TextField(str, "[UnknownType]");
            //                 break;
            //         }
            //     });
            // }
        }
    }
}
#endif
