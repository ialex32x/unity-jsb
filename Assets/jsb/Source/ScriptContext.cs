using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;

namespace QuickJS
{
    public partial class ScriptContext
    {
        public event Action<ScriptContext> OnDestroy;
        public event Action<int> OnAfterDestroy;

        public event Action<ScriptContext, string> OnScriptReloading;
        public event Action<ScriptContext, string> OnScriptReloaded;

        private ScriptRuntime _runtime;
        private int _contextId;
        private JSContext _ctx;
        private AtomCache _atoms;
        private JSStringCache _stringCache;

        private JSValue _moduleCache; // commonjs module cache
        private JSValue _require; // require function object 
        private bool _mainModuleLoaded;
        private bool _isValid;
        private Regex _stRegex;

        private JSValue _globalObject;
        private JSValue _operatorCreate;
        private JSValue _proxyConstructor;
        private JSValue _objectConstructor;
        private JSValue _numberConstructor;
        private JSValue _stringConstructor;
        private JSValue _functionConstructor;

        private bool _isReloading;
        private List<string> _waitForReloadModules;

        private TypeRegister _currentTypeRegister;

        // id = context slot index + 1
        public int id { get { return _contextId; } }

        public ScriptContext(ScriptRuntime runtime, int contextId)
        {
            _isValid = true;
            _runtime = runtime;
            _contextId = contextId;
            _ctx = JSApi.JS_NewContext(_runtime);
            JSApi.JS_SetContextOpaque(_ctx, (IntPtr)_contextId);
            JSApi.JS_AddIntrinsicOperators(_ctx);
            _atoms = new AtomCache(_ctx);
            _stringCache = new JSStringCache(_ctx);
            _moduleCache = JSApi.JS_NewObject(_ctx);
            _globalObject = JSApi.JS_GetGlobalObject(_ctx);
            _objectConstructor = JSApi.JS_GetProperty(_ctx, _globalObject, JSApi.JS_ATOM_Object);
            _numberConstructor = JSApi.JS_GetProperty(_ctx, _globalObject, JSApi.JS_ATOM_Number);
            _proxyConstructor = JSApi.JS_GetProperty(_ctx, _globalObject, JSApi.JS_ATOM_Proxy);
            _stringConstructor = JSApi.JS_GetProperty(_ctx, _globalObject, JSApi.JS_ATOM_String);
            _functionConstructor = JSApi.JS_GetProperty(_ctx, _globalObject, JSApi.JS_ATOM_Function);
            _operatorCreate = JSApi.JS_UNDEFINED;

            if (JSApi.JS_ATOM_Operators.IsValid)
            {
                var operators = JSApi.JS_GetProperty(_ctx, _globalObject, JSApi.JS_ATOM_Operators);
                if (!operators.IsNullish())
                {
                    if (operators.IsException())
                    {
                        _ctx.print_exception();
                    }
                    else
                    {
                        var create = JSApi.JS_GetProperty(_ctx, operators, GetAtom("create"));
                        JSApi.JS_FreeValue(_ctx, operators);
                        if (create.IsException())
                        {
                            _ctx.print_exception();
                        }
                        else
                        {
                            if (JSApi.JS_IsFunction(_ctx, create) == 1)
                            {
                                _operatorCreate = create;

                                // Function.prototype[Symbol.operatorSet] = Operators.create();
                                CreateDefaultOperators(_functionConstructor);
                            }
                            else
                            {
                                JSApi.JS_FreeValue(_ctx, create);
                            }
                        }
                    }
                }
            }
        }

        public void ReleaseTypeRegister(TypeRegister register)
        {
            _currentTypeRegister = null;
        }

        public TypeRegister CreateTypeRegister()
        {
            if (_currentTypeRegister == null)
            {
                _currentTypeRegister = new TypeRegister(this);
            }
            else
            {
                _currentTypeRegister.AddRef();
            }

            return _currentTypeRegister;
        }

        private unsafe void CreateDefaultOperators(JSValue constructor)
        {
            if (!_operatorCreate.IsNullish())
            {
                var rval = JSApi.JS_Call(_ctx, _operatorCreate);
                if (rval.IsException())
                {
                    var ex = _ctx.GetExceptionString();
                    GetLogger()?.Write(LogLevel.Error, ex);
                }
                else
                {
                    JSApi.JS_DefinePropertyValue(_ctx, constructor, JSApi.JS_ATOM_Symbol_operatorSet, rval, JSPropFlags.DEFAULT);
                }
            }
        }

        public bool IsValid()
        {
            lock (this)
            {
                return _isValid;
            }
        }

        public IAsyncManager GetAsyncManager()
        {
            return _isValid ? _runtime.GetAsyncManager() : null;
        }

        public TimerManager GetTimerManager()
        {
            return _runtime.GetTimerManager();
        }

        public IScriptLogger GetLogger()
        {
            return _runtime.GetLogger();
        }

        public TypeDB GetTypeDB()
        {
            return _runtime.GetTypeDB();
        }

        public ObjectCache GetObjectCache()
        {
            return _runtime.GetObjectCache();
        }

        public ScriptRuntime GetRuntime()
        {
            return _runtime;
        }

        public bool IsContext(JSContext ctx)
        {
            return ctx == _ctx;
        }

        //NOTE: 返回值不需要释放, context 销毁时会自动释放所管理的 Atom
        public JSAtom GetAtom(string name)
        {
            return _atoms.GetAtom(name);
        }

        public JSStringCache GetStringCache()
        {
            return _stringCache;
        }

        public void Destroy()
        {
            lock (this)
            {
                if (!_isValid)
                {
                    return;
                }
                _isValid = false;
            }

            try
            {
                OnDestroy?.Invoke(this);
            }
            catch (Exception e)
            {
                _runtime.GetLogger()?.WriteException(e);
            }
            _stringCache.Destroy();
            _atoms.Clear();

            JSApi.JS_FreeValue(_ctx, _proxyConstructor);
            JSApi.JS_FreeValue(_ctx, _objectConstructor);
            JSApi.JS_FreeValue(_ctx, _numberConstructor);
            JSApi.JS_FreeValue(_ctx, _stringConstructor);
            JSApi.JS_FreeValue(_ctx, _functionConstructor);
            JSApi.JS_FreeValue(_ctx, _globalObject);
            JSApi.JS_FreeValue(_ctx, _operatorCreate);

            JSApi.JS_FreeValue(_ctx, _moduleCache);
            JSApi.JS_FreeValue(_ctx, _require);
            JSApi.JS_FreeContext(_ctx);
            var id = _contextId;
            _contextId = -1;
            _ctx = JSContext.Null;
            try
            {
                OnAfterDestroy?.Invoke(id);
            }
            catch (Exception e)
            {
                _runtime.GetLogger()?.WriteException(e);
            }
        }

        public void FreeValue(JSValue value)
        {
            _runtime.FreeValue(value);
        }

        public void FreeValues(JSValue[] values)
        {
            _runtime.FreeValues(values);
        }

        public unsafe void FreeValues(int argc, JSValue* values)
        {
            _runtime.FreeValues(argc, values);
        }

        ///<summary>
        /// 获取全局对象 (增加引用计数)
        ///</summary>
        public JSValue GetGlobalObject()
        {
            return JSApi.JS_DupValue(_ctx, _globalObject);
        }

        ///<summary>
        /// 获取 string.constructor (增加引用计数)
        ///</summary>
        public JSValue GetStringConstructor()
        {
            return JSApi.JS_DupValue(_ctx, _stringConstructor);
        }

        public JSValue GetFunctionConstructor()
        {
            return JSApi.JS_DupValue(_ctx, _functionConstructor);
        }

        ///<summary>
        /// 获取 number.constructor (增加引用计数)
        ///</summary>
        public JSValue GetNumberConstructor()
        {
            return JSApi.JS_DupValue(_ctx, _numberConstructor);
        }

        public JSValue GetObjectConstructor()
        {
            return JSApi.JS_DupValue(_ctx, _objectConstructor);
        }

        public JSValue GetProxyConstructor()
        {
            return JSApi.JS_DupValue(_ctx, _proxyConstructor);
        }

        public bool CheckNumberType(JSValue jsValue)
        {
            //TODO: 是否成立? 否则需要使用 jsapi equals
            if (jsValue == _numberConstructor)
            {
                return true;
            }

            return false;
        }

        public bool CheckStringType(JSValue jsValue)
        {
            //TODO: 是否成立? 否则需要使用 jsapi equals
            if (jsValue == _stringConstructor)
            {
                return true;
            }

            return false;
        }

        ///<summary>
        /// 获取 operator.create (增加引用计数)
        ///</summary>
        public JSValue GetOperatorCreate()
        {
            return JSApi.JS_DupValue(_ctx, _operatorCreate);
        }

        //TODO: 改为消耗 exports_obj 计数
        // for special resolver (json/static) use
        // no specified parent module assignment
        public JSValue _new_commonjs_resolver_module(string module_id, string resolvername, JSValue exports_obj, bool loaded)
        {
            return _new_commonjs_module_entry(null, module_id, module_id, resolvername, exports_obj, loaded);
        }

        //TODO: 改为消耗 exports_obj 计数
        // for source script use
        public JSValue _new_commonjs_script_module(string parent_module_id, string module_id, string filename, JSValue exports_obj, bool loaded)
        {
            return _new_commonjs_module_entry(parent_module_id, module_id, filename, "source", exports_obj, loaded);
        }

        //TODO: 改为消耗 exports_obj 计数
        //NOTE: 返回值需要调用者 free
        public JSValue _new_commonjs_module_entry(string parent_module_id, string module_id, string filename, string resolvername, JSValue exports_obj, bool loaded)
        {
            var module_obj = JSApi.JS_NewObject(_ctx);
            var module_id_atom = GetAtom(module_id);
            var module_id_obj = JSApi.JS_AtomToString(_ctx, module_id_atom);
            var filename_atom = GetAtom(filename);
            var resolvername_atom = GetAtom(resolvername);

            JSApi.JS_SetProperty(_ctx, _moduleCache, module_id_atom, JSApi.JS_DupValue(_ctx, module_obj));
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("id"), JSApi.JS_DupValue(_ctx, module_id_obj));
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("filename"), JSApi.JS_AtomToString(_ctx, filename_atom));
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("cache"), JSApi.JS_DupValue(_ctx, _moduleCache));
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("loaded"), JSApi.JS_NewBool(_ctx, loaded));
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("exports"), JSApi.JS_DupValue(_ctx, exports_obj));
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("resolvername"), JSApi.JS_AtomToString(_ctx, resolvername_atom));
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("children"), JSApi.JS_NewArray(_ctx));
            JSApi.JS_FreeValue(_ctx, module_id_obj);

            // set parent/children here
            if (!string.IsNullOrEmpty(parent_module_id))
            {
                JSValue parent_mod_obj;
                if (LoadModuleCache(parent_module_id, out parent_mod_obj))
                {
                    var children_obj = JSApi.JS_GetProperty(_ctx, parent_mod_obj, GetAtom("children"));
                    if (JSApi.JS_IsArray(_ctx, children_obj) == 1)
                    {
                        var lengthVal = JSApi.JS_GetProperty(_ctx, children_obj, JSApi.JS_ATOM_length);
                        if (lengthVal.IsNumber())
                        {
                            int length;
                            if (JSApi.JS_ToInt32(_ctx, out length, lengthVal) == 0 && length >= 0)
                            {
                                JSApi.JS_SetPropertyUint32(_ctx, children_obj, (uint)length, JSApi.JS_DupValue(_ctx, module_obj));
                            }
                        }
                        JSApi.JS_FreeValue(_ctx, lengthVal);
                    }
                    JSApi.JS_FreeValue(_ctx, children_obj);
                    JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("parent"), parent_mod_obj);
                }
            }

            return module_obj;
        }

        // retrn the main module value (commonjs module) directly
        public JSValue _dup_commonjs_main_module()
        {
            return JSApi.JS_GetProperty(_ctx, _require, GetAtom("main"));
        }

        public void ForEachModuleExport(Func<JSAtom, JSAtom, JSValue, bool> callback)
        {
            JSApi.ForEachProperty(_ctx, _moduleCache, (mod_id, mod_obj) =>
            {
                var exports = JSApi.JS_GetProperty(_ctx, mod_obj, GetAtom("exports"));
                if (exports.IsObject())
                {
                    var ret = JSApi.ForEachProperty(_ctx, exports, (export_id, export_obj) => callback(mod_id, export_id, export_obj));
                    JSApi.JS_FreeValue(_ctx, exports);
                    return ret;
                }
                return false;
            });
        }

        public bool LoadModuleCacheExports(string module_id, string key, out JSValue value)
        {
            JSValue mod_obj;

            value = JSApi.JS_UNDEFINED;
            if (LoadModuleCache(module_id, out mod_obj))
            {
                var exports = JSApi.JS_GetProperty(_ctx, mod_obj, GetAtom("exports"));

                if (exports.IsObject())
                {
                    value = JSApi.JS_GetProperty(_ctx, exports, GetAtom(key));
                }

                JSApi.JS_FreeValue(_ctx, exports);
                JSApi.JS_FreeValue(_ctx, mod_obj);
            }

            return !value.IsUndefined();
        }

        public bool LoadModuleCache(string module_id, out JSValue value)
        {
            var prop = GetAtom(module_id);
            var mod = JSApi.JS_GetProperty(_ctx, _moduleCache, prop);
            if (mod.IsObject())
            {
                value = mod;
                return true;
            }
            value = JSApi.JS_UNDEFINED;
            JSApi.JS_FreeValue(_ctx, mod);
            return false;
        }

        public void BeginModuleReload()
        {
            if (!_isReloading)
            {
                _isReloading = true;
                _waitForReloadModules = new List<string>();
            }
        }

        public void MarkModuleReload(string module_id)
        {
            if (_isReloading && !_waitForReloadModules.Contains(module_id))
            {
                _waitForReloadModules.Add(module_id);
            }
        }

        public void EndModuleReload()
        {
            if (!_isReloading)
            {
                return;
            }

            while (_waitForReloadModules.Count > 0)
            {
                var module_id = _waitForReloadModules[0];

                if (!_runtime.ReloadModule(this, module_id))
                {
                    _waitForReloadModules.Remove(module_id);
                }
            }

            _isReloading = false;
            _waitForReloadModules = null;
        }

        public void RaiseScriptReloadingEvent_throw(string resolved_id)
        {
            OnScriptReloading?.Invoke(this, resolved_id);
        }

        public void RaiseScriptReloadedEvent_throw(string resolved_id)
        {
            OnScriptReloaded?.Invoke(this, resolved_id);
        }

#if !JSB_UNITYLESS
        public bool CheckModuleId(Unity.JSScriptRef scriptRef, string resolved_id)
        {
            return _runtime.ResolveModuleId(this, "", scriptRef.modulePath) == resolved_id;
        }
#endif

        public bool TryGetModuleForReloading(string resolved_id, out JSValue module_obj)
        {
            if (_waitForReloadModules != null && _waitForReloadModules.Contains(resolved_id))
            {
                _waitForReloadModules.Remove(resolved_id);
                if (LoadModuleCache(resolved_id, out module_obj))
                {
                    return true;
                }
            }

            module_obj = JSApi.JS_UNDEFINED;
            return false;
        }

        // this method will consume the module_obj refcount 
        public unsafe JSValue LoadModuleFromSource(byte[] source, string resolved_id, JSValue module_obj)
        {
            object unused;
            return LoadModuleFromSource(source, resolved_id, module_obj, null, out unused);
        }

        public unsafe JSValue LoadModuleFromSource(byte[] source, string resolved_id, JSValue module_obj, Type expectedReturnType, out object expectedReturnValue)
        {
            var context = this;
            var ctx = _ctx;
            var dirname = PathUtils.GetDirectoryName(resolved_id);
            var resolved_id_bytes = Utils.TextUtils.GetNullTerminatedBytes(resolved_id);
            var filename_obj = JSApi.JS_GetProperty(ctx, module_obj, context.GetAtom("filename"));
            var module_id_atom = context.GetAtom(resolved_id);
            var dirname_atom = context.GetAtom(dirname);
            JSValue require_obj;
            JSValue main_mod_obj;
            if (_mainModuleLoaded)
            {
                require_obj = JSApi.JSB_NewCFunction(ctx, ScriptRuntime.module_require, context.GetAtom("require"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
                main_mod_obj = context._dup_commonjs_main_module();
            }
            else
            {
                _mainModuleLoaded = true;
                require_obj = JSApi.JS_DupValue(_ctx, _require);
                main_mod_obj = JSApi.JS_DupValue(_ctx, module_obj);
            }
            var dirname_obj = JSApi.JS_AtomToString(ctx, dirname_atom);
            var exports_obj = JSApi.JS_GetProperty(ctx, module_obj, context.GetAtom("exports"));

            expectedReturnValue = null;
            JSApi.JS_SetProperty(ctx, require_obj, context.GetAtom("moduleId"), JSApi.JS_AtomToString(ctx, module_id_atom));
            JSApi.JS_SetProperty(ctx, require_obj, context.GetAtom("main"), main_mod_obj);

            var require_argv = new JSValue[5] { exports_obj, require_obj, module_obj, filename_obj, dirname_obj, };

            var tagValue = ScriptRuntime.TryReadByteCodeTagValue(source);
            if (tagValue == ScriptRuntime.BYTECODE_COMMONJS_MODULE_TAG)
            {
                // bytecode
                fixed (byte* intput_ptr = source)
                {
                    var bytecodeFunc = JSApi.JS_ReadObject(ctx, intput_ptr + sizeof(uint), source.Length - sizeof(uint), JSApi.JS_READ_OBJ_BYTECODE);

                    if (bytecodeFunc.tag == JSApi.JS_TAG_FUNCTION_BYTECODE)
                    {
                        var func_val = JSApi.JS_EvalFunction(ctx, bytecodeFunc); // it's CallFree (bytecodeFunc)
                        if (JSApi.JS_IsFunction(ctx, func_val) != 1)
                        {
                            JSApi.JS_FreeValue(ctx, func_val);
                            JSApi.JS_FreeValue(ctx, require_argv);
                            return JSApi.JS_ThrowInternalError(ctx, "failed to require bytecode module");
                        }

                        var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED, require_argv.Length, require_argv);
                        JSApi.JS_FreeValue(ctx, func_val);
                        if (rval.IsException())
                        {
                            JSApi.JS_FreeValue(ctx, require_argv);
                            return rval;
                        }

                        // success
                        if (expectedReturnType != null)
                        {
                            Values.js_get_var(_ctx, rval, expectedReturnType, out expectedReturnValue);
                        }
                        else
                        {
                            expectedReturnValue = null;
                        }
                        JSApi.JS_FreeValue(ctx, rval);
                    }
                    else
                    {
                        JSApi.JS_FreeValue(ctx, bytecodeFunc);
                        JSApi.JS_FreeValue(ctx, require_argv);
                        return JSApi.JS_ThrowInternalError(ctx, "failed to require bytecode module");
                    }
                }
            }
            else
            {
                // source
                var input_bytes = TextUtils.GetShebangNullTerminatedCommonJSBytes(source);
                fixed (byte* input_ptr = input_bytes)
                fixed (byte* resolved_id_ptr = resolved_id_bytes)
                {
                    var input_len = (size_t)(input_bytes.Length - 1);
                    var func_val = JSApi.JS_Eval(ctx, input_ptr, input_len, resolved_id_ptr, JSEvalFlags.JS_EVAL_TYPE_GLOBAL | JSEvalFlags.JS_EVAL_FLAG_STRICT);
                    if (func_val.IsException())
                    {
                        JSApi.JS_FreeValue(ctx, require_argv);
                        return func_val;
                    }

                    if (JSApi.JS_IsFunction(ctx, func_val) == 1)
                    {
                        var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED, require_argv.Length, require_argv);
                        if (rval.IsException())
                        {
                            JSApi.JS_FreeValue(ctx, func_val);
                            JSApi.JS_FreeValue(ctx, require_argv);
                            return rval;
                        }

                        // success
                        if (expectedReturnType != null)
                        {
                            Values.js_get_var(_ctx, rval, expectedReturnType, out expectedReturnValue);
                        }
                        else
                        {
                            expectedReturnValue = null;
                        }
                        JSApi.JS_FreeValue(ctx, rval);
                    }

                    JSApi.JS_FreeValue(ctx, func_val);
                }
            }

            JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, true));
            var exports_ = JSApi.JS_GetProperty(ctx, module_obj, context.GetAtom("exports"));
            JSApi.JS_FreeValue(ctx, require_argv);
            return exports_;
        }

        /// <summary>
        /// 添加全局函数
        /// </summary>
        public void AddFunction(string name, JSCFunction func, int length)
        {
            AddFunction(_globalObject, name, func, length);
        }

        public void AddFunction(JSValue thisObject, string name, JSCFunction func, int length)
        {
            var nameAtom = GetAtom(name);
            var cfun = JSApi.JSB_NewCFunction(_ctx, func, nameAtom, length, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_DefinePropertyValue(_ctx, thisObject, nameAtom, cfun, JSPropFlags.JS_PROP_C_W_E);
        }

        public static ClassDecl Bind(TypeRegister register)
        {
            var ns_jsb = register.CreateClass("JSBObject");

            ns_jsb.AddFunction("DoFile", _DoFile, 1);
            ns_jsb.AddFunction("AddSearchPath", _AddSearchPath, 1);
            ns_jsb.AddFunction("Yield", yield_func, 1);
            ns_jsb.AddFunction("ToArray", to_js_array, 1);
            ns_jsb.AddFunction("ToArrayBuffer", to_js_array_buffer, 1);
            ns_jsb.AddFunction("ToBytes", to_cs_bytes, 1);
            ns_jsb.AddFunction("ToFunction", to_js_function, 1);
            ns_jsb.AddFunction("ToDelegate", to_cs_delegate, 1);
            ns_jsb.AddFunction("Import", js_import_type, 2);
            ns_jsb.AddFunction("GC", _gc, 0);
            ns_jsb.AddFunction("SetDisposable", _set_disposable, 2);
            ns_jsb.AddFunction("AddCacheString", _add_cache_string, 1);
            ns_jsb.AddFunction("RemoveCacheString", _remove_cache_string, 1);
            ns_jsb.AddFunction("Sleep", _sleep, 1);
            ns_jsb.AddFunction("AddModule", _add_module, 2);
            ns_jsb.AddFunction("Now", _now, 0);
            {
                var ns_hotfix = register.CreateClass("JSBHotfix");
                ns_hotfix.AddFunction("replace_single", hotfix_replace_single, 2);
                ns_hotfix.AddFunction("before_single", hotfix_before_single, 2);
                // ns_hotfix.AddFunction("replace", hotfix_replace, 2);
                // ns_hotfix.AddFunction("before", hotfix_before);
                // ns_hotfix.AddFunction("after", hotfix_after);

                ns_jsb.AddValue("hotfix", ns_hotfix.GetConstructor());
            }
            {
                var ns_ModuleManager = register.CreateClass("ModuleManager");
                ns_ModuleManager.AddFunction("BeginReload", ModuleManager_BeginReload, 0);
                ns_ModuleManager.AddFunction("MarkReload", ModuleManager_MarkReload, 1);
                ns_ModuleManager.AddFunction("EndReload", ModuleManager_EndReload, 0);

                ns_jsb.AddValue("ModuleManager", ns_ModuleManager.GetConstructor());
            }
            return ns_jsb;
        }

        public void EvalSource(string source, string fileName)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(source);
            EvalSource(bytes, fileName, typeof(void));
        }

        public T EvalSource<T>(string source, string fileName)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(source);
            return (T)EvalSource(bytes, fileName, typeof(T));
        }

        public void EvalSource(byte[] source, string fileName)
        {
            EvalSource(source, fileName, typeof(void));
        }

        public T EvalSource<T>(byte[] source, string fileName)
        {
            return (T)EvalSource(source, fileName, typeof(T));
        }

        public object EvalSource(byte[] source, string fileName, Type returnType)
        {
            var jsValue = ScriptRuntime.EvalSource(_ctx, source, fileName, false);
            if (JSApi.JS_IsException(jsValue))
            {
                var ex = _ctx.GetExceptionString();
                throw new JSException(ex, fileName);
            }
            object retObject;
            Values.js_get_var(_ctx, jsValue, returnType, out retObject);
            JSApi.JS_FreeValue(_ctx, jsValue);
            return retObject;
        }

        public void RegisterBuiltins()
        {
            var ctx = (JSContext)this;
            var global_object = this.GetGlobalObject();
            {
                _mainModuleLoaded = false;
                _require = JSApi.JSB_NewCFunction(ctx, ScriptRuntime.module_require, GetAtom("require"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
                JSApi.JS_SetProperty(ctx, _require, GetAtom("moduleId"), JSApi.JS_NewString(ctx, ""));
                JSApi.JS_SetProperty(ctx, _require, GetAtom("cache"), JSApi.JS_DupValue(ctx, _moduleCache));
                JSApi.JS_SetProperty(ctx, global_object, GetAtom("require"), JSApi.JS_DupValue(ctx, _require));

                JSApi.JS_SetPropertyStr(ctx, global_object, "print", JSApi.JS_NewCFunctionMagic(ctx, _print, "print", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                var console = JSApi.JS_NewObject(ctx);
                {
                    JSApi.JS_SetPropertyStr(ctx, console, "log", JSApi.JS_NewCFunctionMagic(ctx, _print, "log", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                    JSApi.JS_SetPropertyStr(ctx, console, "info", JSApi.JS_NewCFunctionMagic(ctx, _print, "info", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                    JSApi.JS_SetPropertyStr(ctx, console, "debug", JSApi.JS_NewCFunctionMagic(ctx, _print, "debug", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                    JSApi.JS_SetPropertyStr(ctx, console, "warn", JSApi.JS_NewCFunctionMagic(ctx, _print, "warn", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 1));
                    JSApi.JS_SetPropertyStr(ctx, console, "error", JSApi.JS_NewCFunctionMagic(ctx, _print, "error", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 2));
                    JSApi.JS_SetPropertyStr(ctx, console, "assert", JSApi.JS_NewCFunctionMagic(ctx, _print, "assert", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 3));
                    JSApi.JS_SetPropertyStr(ctx, console, "trace", JSApi.JS_NewCFunctionMagic(ctx, _print, "trace", 0, JSCFunctionEnum.JS_CFUNC_generic_magic, -1));
                }
                JSApi.JS_SetPropertyStr(ctx, global_object, "console", console);
            }
            JSApi.JS_FreeValue(ctx, global_object);
        }

        private string js_source_position(JSContext ctx, string funcName, string fileName, int lineNumber)
        {
            return $"{funcName} ({fileName}:{lineNumber})";
        }

        public void AppendStacktrace(StringBuilder sb)
        {
            var ctx = _ctx;
            var globalObject = JSApi.JS_GetGlobalObject(ctx);
            var errorConstructor = JSApi.JS_GetProperty(ctx, globalObject, JSApi.JS_ATOM_Error);
            var errorObject = JSApi.JS_CallConstructor(ctx, errorConstructor);
            var stackValue = JSApi.JS_GetProperty(ctx, errorObject, JSApi.JS_ATOM_stack);
            var stack = JSApi.GetString(ctx, stackValue);

            if (!string.IsNullOrEmpty(stack))
            {
                var errlines = stack.Split('\n');
                if (_stRegex == null)
                {
                    _stRegex = new Regex(@"^\s+at\s(.+)\s\((.+\.js):(\d+)\)(.*)$", RegexOptions.Compiled);
                }
                for (var i = 0; i < errlines.Length; i++)
                {
                    var line = errlines[i];
                    var matches = _stRegex.Matches(line);
                    if (matches.Count == 1)
                    {
                        var match = matches[0];
                        if (match.Groups.Count >= 4)
                        {
                            var funcName = match.Groups[1].Value;
                            var fileName = match.Groups[2].Value;
                            var lineNumber = 0;
                            int.TryParse(match.Groups[3].Value, out lineNumber);
                            var extra = match.Groups.Count >= 5 ? match.Groups[4].Value : "";
                            var sroucePosition = (_runtime.OnSourceMap ?? js_source_position)(ctx, funcName, fileName, lineNumber);
                            sb.AppendLine($"    at {sroucePosition}{extra}");
                            continue;
                        }
                    }
                    sb.AppendLine(line);
                }
            }

            JSApi.JS_FreeValue(ctx, stackValue);
            JSApi.JS_FreeValue(ctx, errorObject);
            JSApi.JS_FreeValue(ctx, errorConstructor);
            JSApi.JS_FreeValue(ctx, globalObject);
        }

        public static implicit operator JSContext(ScriptContext sc)
        {
            return sc._ctx;
        }
    }
}