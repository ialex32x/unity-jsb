using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public struct ClassDecl
    {
        public TypeRegister _register;
        private ScriptContext _ctx;
        public JSValue _ctor;
        public JSValue _proto;

        private JSValue _self_operators;
        private JSValue _left_operators;
        private JSValue _right_operators;

        public JSAtom GetAtom(string name)
        {
            return _register.GetAtom(name);
        }

        public ClassDecl(TypeRegister register, JSValue ctorVal, JSValue protoVal)
        {
            _register = register;
            _ctx = _register.GetContext();
            _ctor = JSApi.JS_DupValue(_ctx, ctorVal);
            _proto = JSApi.JS_DupValue(_ctx, protoVal);
            _self_operators = JSApi.JS_UNDEFINED;
            _left_operators = JSApi.JS_UNDEFINED;
            _right_operators = JSApi.JS_UNDEFINED;
        }

        public void Close()
        {
            var ctx = (JSContext)_ctx;
            var opc =
                (_self_operators.IsNullish() ? 0 : 1) +
                (_left_operators.IsNullish() ? 0 : 1) +
                (_right_operators.IsNullish() ? 0 : 1);

            if (opc > 0)
            {
                // 提交运算符重载
                if (_self_operators.IsNullish())
                {
                    // 保证 self 运算符定义存在
                    _self_operators = JSApi.JS_NewObject(ctx);
                    opc++;
                }

                var globalObj = JSApi.JS_GetGlobalObject(ctx);
                var operators = JSApi.JS_GetProperty(ctx, globalObj, JSApi.JS_ATOM_Operators);
                if (!operators.IsNullish())
                {
                    if (operators.IsException())
                    {
                        ctx.print_exception();
                    }
                    else
                    {
                        var create = JSApi.JS_GetProperty(ctx, operators, GetAtom("create"));
                        var js_operators = new JSValue[opc];
                        js_operators[0] = _self_operators;
                        if (!_left_operators.IsNullish()) js_operators[--opc] = _left_operators;
                        if (!_right_operators.IsNullish()) js_operators[--opc] = _right_operators;

                        JSValue rval;
                        unsafe
                        {
                            fixed (JSValue* ptr = js_operators)
                            {
                                rval = JSApi.JS_Call(ctx, create, JSApi.JS_UNDEFINED, 3, ptr);
                                if (rval.IsException())
                                {
                                    ctx.print_exception();
                                }
                                else
                                {
                                    JSApi.JS_DefinePropertyValue(ctx, _proto, JSApi.JS_ATOM_Symbol_operatorSet, rval, JSPropFlags.DEFAULT);
                                }
                            }
                        }

                        JSApi.JS_FreeValue(ctx, create);
                    }
                }
                JSApi.JS_FreeValue(ctx, operators);
                JSApi.JS_FreeValue(ctx, globalObj);
            }

            JSApi.JS_FreeValue(ctx, _self_operators);
            JSApi.JS_FreeValue(ctx, _left_operators);
            JSApi.JS_FreeValue(ctx, _right_operators);

            JSApi.JS_FreeValue(ctx, _ctor);
            JSApi.JS_FreeValue(ctx, _proto);
            _ctor = JSApi.JS_UNDEFINED;
            _proto = JSApi.JS_UNDEFINED;
            _ctx = null;
        }

        public void AddSelfOperator(string op, JSCFunction func, int length)
        {
            if (_self_operators.IsNullish())
            {
                _self_operators = JSApi.JS_NewObject(_ctx);
            }
            var funcVal = JSApi.JS_NewCFunction(_ctx, func, op, length);
            JSApi.JS_DefinePropertyValue(_ctx, _self_operators, GetAtom(op), funcVal, JSPropFlags.JS_PROP_C_W_E);
        }

        public void AddLeftOperator(string op, JSCFunction func, int length, string leftTypeName)
        {
            if (_left_operators.IsNullish())
            {
                _left_operators = JSApi.JS_NewObject(_ctx);
                JSApi.JS_SetProperty(_ctx, _left_operators, GetAtom("left"), JSApi.JS_NewString(_ctx, leftTypeName));
            }
            var funcVal = JSApi.JS_NewCFunction(_ctx, func, op, length);
            JSApi.JS_DefinePropertyValue(_ctx, _left_operators, GetAtom(op), funcVal, JSPropFlags.JS_PROP_C_W_E);
        }

        public void AddRightOperator(string op, JSCFunction func, int length, string rightTypeName)
        {
            if (_right_operators.IsNullish())
            {
                _right_operators = JSApi.JS_NewObject(_ctx);
                JSApi.JS_SetProperty(_ctx, _left_operators, GetAtom("right"), JSApi.JS_NewString(_ctx, rightTypeName));
            }
            var funcVal = JSApi.JS_NewCFunction(_ctx, func, op, length);
            JSApi.JS_DefinePropertyValue(_ctx, _right_operators, GetAtom(op), funcVal, JSPropFlags.JS_PROP_C_W_E);
        }

        public void AddMethod(bool bStatic, string name, JSCFunctionMagic func, int length, int magic)
        {
            var nameAtom = _register.GetAtom(name);
            var funcVal = JSApi.JSB_NewCFunctionMagic(_ctx, func, nameAtom, length, JSCFunctionEnum.JS_CFUNC_generic_magic,
                magic);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.JS_PROP_C_W_E);
        }

        public void AddMethod(bool bStatic, string name, JSCFunction func)
        {
            var nameAtom = _register.GetAtom(name);
            var funcVal = JSApi.JSB_NewCFunction(_ctx, func, nameAtom, 0, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.JS_PROP_C_W_E);
        }

        public void AddMethod(bool bStatic, string name, JSCFunction func, int length)
        {
            var nameAtom = _register.GetAtom(name);
            var funcVal = JSApi.JSB_NewCFunction(_ctx, func, nameAtom, length, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_DefinePropertyValue(_ctx, bStatic ? _ctor : _proto, nameAtom, funcVal, JSPropFlags.JS_PROP_C_W_E);
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