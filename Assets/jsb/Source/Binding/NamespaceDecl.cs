using System;
using QuickJS.Native;

namespace QuickJS.Binding
{
    public struct NamespaceDecl
    {
        private TypeRegister _register;
        private JSValue _nsValue;

        public NamespaceDecl(TypeRegister register, JSValue jsNsValue)
        {
            _register = register;
            _nsValue = jsNsValue;
        }

        public void Close()
        {
            JSApi.JS_FreeValue(_register, _nsValue);
            _nsValue = JSApi.JS_UNDEFINED;
        }

        public void AddFunction(string name, JSCFunctionMagic func, int length)
        {
            var ctx = _register.GetContext();
            var magic = 0;
            var cfun = JSApi.JS_NewCFunctionMagic(ctx, func, name, length, JSCFunctionEnum.JS_CFUNC_generic_magic,
                magic);
            JSApi.JS_DefinePropertyValueStr(ctx, _nsValue, name, cfun, JSPropFlags.JS_PROP_C_W_E);
        }

        public void AddFunction(string name, JSCFunction func, int length)
        {
            var ctx = _register.GetContext();
            var cfun = JSApi.JS_NewCFunction(ctx, func, name, length);
            JSApi.JS_DefinePropertyValueStr(ctx, _nsValue, name, cfun, JSPropFlags.JS_PROP_C_W_E);
        }

        public ClassDecl CreateClass(string typename, Type type, JSCFunction ctorFunc)
        {
            var ctx = _register.GetContext();
            var protoVal = ctx.NewObject();
            var type_id = _register.Add(type, protoVal);
            var ctorVal =
                JSApi.JS_NewCFunction2(ctx, ctorFunc, typename, 0, JSCFunctionEnum.JS_CFUNC_constructor, 0);
            var decl = new ClassDecl(_register, JSApi.JS_DupValue(_register, ctorVal),
                JSApi.JS_DupValue(_register, protoVal));
            JSApi.JS_SetConstructor(ctx, ctorVal, protoVal);
            JSApi.JSB_SetBridgeType(ctx, ctorVal, type_id);
            JSApi.JS_DefinePropertyValueStr(ctx, _nsValue, typename, ctorVal,
                JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            JSApi.JS_FreeValue(ctx, protoVal);
            return decl;
        }

        public ClassDecl CreateClass(string typename, Type type, JSCFunctionMagic ctorFunc, int magic)
        {
            var ctx = _register.GetContext();
            var protoVal = ctx.NewObject();
            var type_id = _register.Add(type, protoVal);
            var ctorVal =
                JSApi.JS_NewCFunctionMagic(ctx, ctorFunc, typename, 0, JSCFunctionEnum.JS_CFUNC_constructor_magic,
                    magic);
            var decl = new ClassDecl(_register, JSApi.JS_DupValue(_register, ctorVal),
                JSApi.JS_DupValue(_register, protoVal));
            JSApi.JS_SetConstructor(ctx, ctorVal, protoVal);
            JSApi.JSB_SetBridgeType(ctx, ctorVal, type_id);
            JSApi.JS_DefinePropertyValueStr(ctx, _nsValue, typename, ctorVal,
                JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            JSApi.JS_FreeValue(ctx, protoVal);
            return decl;
        }

        public ClassDecl CreateEnum(string typename, Type type)
        {
            return CreateClass(typename, type, JSApi.class_private_ctor);
        }
    }
}