using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public struct OperatorDef
    {
        public string op;
        public JSCFunction func;
        public int length;

        public OperatorDef(string op, JSCFunction func, int length)
        {
            this.func = func;
            this.length = length;
            this.op = op;
        }
    }

    public struct CrossOperatorDef
    {
        public Type type;
        public List<OperatorDef> operators;

        public CrossOperatorDef(Type type)
        {
            this.type = type;
            this.operators = new List<OperatorDef>();
        }
    }

    public struct ClassDecl
    {
        private TypeRegister _register;
        private ScriptContext _ctx;
        private JSValue _ctor;
        private JSValue _proto;
        private Type _type;

        public JSAtom GetAtom(string name)
        {
            return _register.GetAtom(name);
        }

        public ClassDecl(TypeRegister register, JSValue ctorVal, JSValue protoVal, Type type)
        {
            _type = type;
            _register = register;
            _ctx = _register.GetContext();
            _ctor = JSApi.JS_DupValue(_ctx, ctorVal);
            _proto = JSApi.JS_DupValue(_ctx, protoVal);
        }

        public void Close()
        {
            var ctx = (JSContext)_ctx;

            JSApi.JS_FreeValue(ctx, _ctor);
            JSApi.JS_FreeValue(ctx, _proto);
            _ctor = JSApi.JS_UNDEFINED;
            _proto = JSApi.JS_UNDEFINED;
            _ctx = null;
        }

        public void AddSelfOperator(string op, JSCFunction func, int length)
        {
            _register.RegisterOperator(_type, op, func, length);
        }

        public void AddLeftOperator(string op, JSCFunction func, int length, Type type)
        {
            _register.RegisterOperator(_type, op, func, length, true, type);
        }

        public void AddRightOperator(string op, JSCFunction func, int length, Type type)
        {
            _register.RegisterOperator(_type, op, func, length, false, type);
        }

        public void AddMethod(bool bStatic, string name, JSCFunctionMagic func, int length, int magic)
        {
            var nameAtom = _register.GetAtom(name);
            var funcVal = JSApi.JSB_NewCFunctionMagic(_ctx, func, nameAtom, length, JSCFunctionEnum.JS_CFUNC_generic_magic,
                magic);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.DEFAULT);
        }

        public void AddMethod(bool bStatic, string name, JSCFunction func)
        {
            var nameAtom = _register.GetAtom(name);
            var funcVal = JSApi.JSB_NewCFunction(_ctx, func, nameAtom, 0, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.DEFAULT);
        }

        public void AddMethod(bool bStatic, string name, JSCFunction func, int length)
        {
            var nameAtom = _register.GetAtom(name);
            var funcVal = JSApi.JSB_NewCFunction(_ctx, func, nameAtom, length, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.DEFAULT);
        }

        public void AddStaticEvent(string name, JSCFunction adder, JSCFunction remover)
        {
            var nameAtom = _register.GetAtom(name);
            var op = JSApi.JS_NewObject(_ctx);
            var adderFunc = JSApi.JSB_NewCFunction(_ctx, adder, _register.GetAtom("on"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_SetProperty(_ctx, op, _register.GetAtom("on"), adderFunc);
            var removerFunc = JSApi.JSB_NewCFunction(_ctx, remover, _register.GetAtom("off"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_SetProperty(_ctx, op, _register.GetAtom("off"), removerFunc);
            JSApi.JS_SetProperty(_ctx, _ctor, nameAtom, op);
        }

        public void AddRawMethod(bool bStatic, string name, JSCFunction method)
        {
            var nameAtom = _register.GetAtom(name);
            var db = _register.GetTypeDB();
            var funcVal = db.NewDynamicMethod(nameAtom, method);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.DEFAULT);
        }

        public void AddMethod(bool bStatic, string name, IDynamicMethod method)
        {
            var nameAtom = _register.GetAtom(name);
            var db = _register.GetTypeDB();
            var funcVal = db.NewDynamicMethod(nameAtom, method);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.DEFAULT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddField(bool bStatic, string name, JSGetterCFunction getter, JSSetterCFunction setter)
        {
            AddProperty(bStatic, name, getter, setter);
        }

        public void AddField(bool bStatic, string name, IDynamicField field)
        {
            AddProperty(bStatic, name, field);
        }

        public void AddProperty(bool bStatic, string name, JSGetterCFunction getter, JSSetterCFunction setter)
        {
            var ctx = (JSContext)_ctx;
            var nameAtom = _register.GetAtom(name);
            var getterVal = JSApi.JS_UNDEFINED;
            var setterVal = JSApi.JS_UNDEFINED;
            var flags = JSPropFlags.JS_PROP_CONFIGURABLE | JSPropFlags.JS_PROP_ENUMERABLE;
            if (getter != null)
            {
                flags |= JSPropFlags.JS_PROP_HAS_GET;
                getterVal = JSApi.JSB_NewCFunction(ctx, getter, nameAtom);
            }

            if (setter != null)
            {
                flags |= JSPropFlags.JS_PROP_HAS_SET;
                flags |= JSPropFlags.JS_PROP_WRITABLE;
                setterVal = JSApi.JSB_NewCFunction(ctx, setter, nameAtom);
            }

            var rs = JSApi.JS_DefineProperty(ctx, bStatic ? _ctor : _proto, nameAtom, JSApi.JS_UNDEFINED, getterVal, setterVal, flags);

            if (rs != 1)
            {
                var logger = _register.GetLogger();

                if (logger != null)
                {
                    logger.Write(LogLevel.Error, "define property failed: {0}", ctx.GetExceptionString());
                }
            }
            JSApi.JS_FreeValue(ctx, getterVal);
            JSApi.JS_FreeValue(ctx, setterVal);
        }

        public void AddProperty(bool bStatic, string name, IDynamicField field)
        {
            var ctx = (JSContext)_ctx;
            var nameAtom = _register.GetAtom(name);
            var getterVal = JSApi.JS_UNDEFINED;
            var setterVal = JSApi.JS_UNDEFINED;
            var flags = JSPropFlags.JS_PROP_CONFIGURABLE | JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_HAS_GET | JSPropFlags.JS_PROP_HAS_SET | JSPropFlags.JS_PROP_WRITABLE;
            var db = _register.GetTypeDB();

            db.NewDynamicFieldAccess(nameAtom, field, out getterVal, out setterVal);

            var rs = JSApi.JS_DefineProperty(ctx, bStatic ? _ctor : _proto, nameAtom, JSApi.JS_UNDEFINED, getterVal, setterVal, flags);
            if (rs != 1)
            {
                var logger = _register.GetLogger();

                if (logger != null)
                {
                    logger.Write(LogLevel.Error, "define property failed: {0}", ctx.GetExceptionString());
                }
            }
            JSApi.JS_FreeValue(ctx, getterVal);
            JSApi.JS_FreeValue(ctx, setterVal);
        }

        #region 常量 (静态)
        public void AddConstValue(string name, bool v)
        {
            var val = JSApi.JS_NewBool(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, char v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, byte v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, sbyte v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, short v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, ushort v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, int v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, uint v)
        {
            var val = JSApi.JS_NewUint32(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, double v)
        {
            var val = JSApi.JS_NewFloat64(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, float v)
        {
            var val = JSApi.JS_NewFloat64(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, string v)
        {
            var val = JSApi.JS_NewString(_ctx, v);
            var nameAtom = _register.GetAtom(name);
            JSApi.JS_DefinePropertyValue(_ctx, _ctor, nameAtom, val, JSPropFlags.CONST_VALUE);
        }
        #endregion
    }
}