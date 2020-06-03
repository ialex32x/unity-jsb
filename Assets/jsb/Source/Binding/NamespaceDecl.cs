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

        public ClassDecl CreateClass(string typename, Type type, JSCFunctionMagic ctor)
        {
            var ctx = _register.GetContext();
            var runtime = ctx.GetRuntime();
            var class_id = runtime._def_class_id;
            var proto_val = ctx.NewObject();
            var type_id = _register.Add(type, proto_val);
            var ctor_val =
                JSApi.JS_NewCFunctionMagic(ctx, ctor, typename, 0, JSCFunctionEnum.JS_CFUNC_constructor_magic, type_id);
            var decl = new ClassDecl(_register, JSApi.JS_DupValue(_register, ctor_val), JSApi.JS_DupValue(_register, proto_val));
            JSApi.JS_SetConstructor(ctx, ctor_val, proto_val);
            JSApi.JS_SetClassProto(ctx, class_id, proto_val);
            JSApi.JS_DefinePropertyValueStr(ctx, _nsValue, typename, ctor_val,
                JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            return decl;
        }

        public ClassDecl CreateEnum(string typename, Type type)
        {
            return CreateClass(typename, type, JSApi.class_private_ctor);
        }
    }
}