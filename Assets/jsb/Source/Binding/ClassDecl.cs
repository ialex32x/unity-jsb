using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public struct SelfOperatorDef
    {
        public string op;
        public JSCFunction func;
        public int length;

        public SelfOperatorDef(JSCFunction func, int length, string op)
        {
            this.func = func;
            this.length = length;
            this.op = op;
        }
    }

    public struct OperatorDef
    {
        public string side;
        public Type type;
        public JSCFunction func;
        public int length;
        public string op;

        public OperatorDef(string side, Type type, JSCFunction func, int length, string op)
        {
            this.side = side;
            this.type = type;
            this.func = func;
            this.length = length;
            this.op = op;
        }
    }

    public struct ClassDecl
    {
        private TypeRegister _register;
        private ScriptContext _ctx;
        private JSValue _ctor;
        private JSValue _proto;
        private Type _type;

        private List<SelfOperatorDef> _selfOperators;
        private List<OperatorDef> _operators;

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
            _operators = null;
            _selfOperators = null;
        }

        public void Close()
        {
            var ctx = (JSContext)_ctx;

            if (_operators != null)
            {
                if (_selfOperators == null)
                {
                    UnityEngine.Debug.LogWarning("no self operators?");
                }
                _register.RegisterOperator(_type, JSApi.JS_DupValue(_ctx, _proto), _selfOperators, _operators);
                _operators = null;
            }

            JSApi.JS_FreeValue(ctx, _ctor);
            JSApi.JS_FreeValue(ctx, _proto);
            _ctor = JSApi.JS_UNDEFINED;
            _proto = JSApi.JS_UNDEFINED;
            _ctx = null;
        }

        public void AddSelfOperator(string op, JSCFunction func, int length)
        {
            if (_selfOperators == null)
            {
                _selfOperators = new List<SelfOperatorDef>();
            }
            _selfOperators.Add(new SelfOperatorDef(func, length, op));
        }

        public void AddLeftOperator(string op, JSCFunction func, int length, Type type)
        {
            if (_operators == null)
            {
                _operators = new List<OperatorDef>();
            }
            _operators.Add(new OperatorDef("left", type, func, length, op));
        }

        public void AddRightOperator(string op, JSCFunction func, int length, Type type)
        {
            if (_operators == null)
            {
                _operators = new List<OperatorDef>();
            }
            _operators.Add(new OperatorDef("right", type, func, length, op));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddField(bool bStatic, string name, JSGetterCFunction getter, JSSetterCFunction setter)
        {
            AddProperty(bStatic, name, getter, setter);
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
                getterVal = JSApi.JSB_NewCFunction(ctx, getter, nameAtom, 0, JSCFunctionEnum.JS_CFUNC_getter, 0);
            }

            if (setter != null)
            {
                flags |= JSPropFlags.JS_PROP_HAS_SET;
                flags |= JSPropFlags.JS_PROP_WRITABLE;
                setterVal = JSApi.JSB_NewCFunction(ctx, setter, nameAtom, 1, JSCFunctionEnum.JS_CFUNC_setter, 0);
            }

            var rs = JSApi.JS_DefineProperty(ctx, bStatic ? _ctor : _proto, nameAtom, JSApi.JS_UNDEFINED, getterVal, setterVal,
                flags);
            if (rs != 1)
            {
                var logger = _register.GetLogger();

                logger.Write(LogLevel.Error, "define property failed: {0}", ctx.GetExceptionString());
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