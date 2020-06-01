using System;
using QuickJS.Native;

namespace QuickJS.Binding
{
    public struct ClassDecl
    {
        public TypeRegister _register;
        private ScriptContext _ctx;
        public JSValue _ctor;
        public JSValue _proto;

        public ClassDecl(TypeRegister register, JSValue ctor, JSValue proto)
        {
            _register = register;
            _ctx = _register.GetContext();
            _ctor = ctor;
            _proto = proto;
            JSApi.JS_DupValue(_ctx, _ctor);
            JSApi.JS_DupValue(_ctx, _proto);
        }

        public void Close()
        {
            JSApi.JS_FreeValue(_ctx, _ctor);
            JSApi.JS_FreeValue(_ctx, _proto);
            _ctor = JSApi.JS_UNDEFINED;
            _proto = JSApi.JS_UNDEFINED;
            _ctx = null;
        }

        public void AddMethod(string name, JSCFunctionMagic func, int length, bool bStatic)
        {
            var magic = 0;
            var cfun = JSApi.JS_NewCFunctionMagic(_ctx, func, name, length, JSCFunctionEnum.JS_CFUNC_generic_magic,
                magic);
            JSApi.JS_DefinePropertyValueStr(_ctx, bStatic ? _ctor : _proto, name, cfun, JSPropFlags.JS_PROP_C_W_E);
        }

        public void AddField(string name, JSCFunctionMagic getter, JSCFunctionMagic setter, bool bStatic)
        {
            // js 层面field与property绑定代码结构完全一致
            AddProperty(name, getter, setter, bStatic);
        }

        public void AddProperty(string name, JSCFunctionMagic getter, JSCFunctionMagic setter, bool bStatic)
        {
            // [ctor, prototype]
            var getterVal = JSApi.JS_UNDEFINED;
            var setterVal = JSApi.JS_UNDEFINED;
            var flags = JSPropFlags.JS_PROP_HAS_CONFIGURABLE | JSPropFlags.JS_PROP_HAS_ENUMERABLE;
            if (getter != null)
            {
                flags |= JSPropFlags.JS_PROP_HAS_GET;
                getterVal = JSApi.JS_NewCFunctionMagic(_ctx, getter, name, 0, JSCFunctionEnum.JS_CFUNC_getter_magic, 0);
            }

            if (setter != null)
            {
                flags |= JSPropFlags.JS_PROP_HAS_SET;
                setterVal = JSApi.JS_NewCFunctionMagic(_ctx, setter, name, 0, JSCFunctionEnum.JS_CFUNC_setter_magic, 0);
            }

            // [ctor, prototype, name, ?getter, ?setter]
            var atom = JSApi.JS_NewAtom(_ctx, name);
            JSApi.JS_DefineProperty(_ctx, bStatic ? _ctor : _proto, atom, JSApi.JS_UNDEFINED, getterVal, setterVal,
                flags);
            JSApi.JS_FreeAtom(_ctx, atom);
        }

        public void AddConstValue(string name, bool v)
        {
            var val = JSApi.JS_NewBool(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, char v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, byte v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, sbyte v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, short v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, ushort v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, int v)
        {
            var val = JSApi.JS_NewInt32(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        // always static
        public void AddConstValue(string name, uint v)
        {
            var val = JSApi.JS_NewUint32(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        // always static
        public void AddConstValue(string name, double v)
        {
            var val = JSApi.JS_NewFloat64(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        public void AddConstValue(string name, float v)
        {
            var val = JSApi.JS_NewFloat64(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }

        // always static
        public void AddConstValue(string name, string v)
        {
            var val = JSApi.JS_NewString(_ctx, v);
            JSApi.JS_DefinePropertyValueStr(_ctx, _ctor, name, val, JSPropFlags.CONST_VALUE);
        }
    }
}