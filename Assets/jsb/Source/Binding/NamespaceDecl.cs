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

        public void Copy(string oldName, string newName)
        {
            var ctx = (JSContext) _register.GetContext();
            var oldVal = JSApi.JS_GetPropertyStr(ctx, _nsValue, oldName);
            if (JSApi.JS_IsException(oldVal))
            {
                ctx.print_exception();
            }
            else
            {
                JSApi.JS_SetPropertyStr(ctx, _nsValue, newName, oldVal);
            }
        }

        public void AddFunction(string name, JSCFunction func, int length)
        {
            var ctx = _register.GetContext();
            var nameAtom = _register.GetAtom(name);
            var cfun = JSApi.JSB_NewCFunction(ctx, func, nameAtom, length, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_DefinePropertyValue(ctx, _nsValue, nameAtom, cfun, JSPropFlags.JS_PROP_C_W_E);
        }

        public ClassDecl CreateClass(string typename, Type type, JSCFunctionMagic ctorFunc)
        {
            var nameAtom = _register.GetAtom(typename);
            JSContext ctx = _register.GetContext();
            var protoVal = JSApi.JS_NewObject(ctx);
            var type_id = _register.RegisterType(type, protoVal);
            var ctorVal =
                JSApi.JSB_NewCFunctionMagic(ctx, ctorFunc, nameAtom, 0, JSCFunctionEnum.JS_CFUNC_constructor_magic, type_id);
            var decl = new ClassDecl(_register, ctorVal, protoVal, type);
            JSApi.JS_SetConstructor(ctx, ctorVal, protoVal);
            JSApi.JSB_SetBridgeType(ctx, ctorVal, type_id);
            JSApi.JS_DefinePropertyValue(ctx, _nsValue, nameAtom, ctorVal,
                JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            // UnityEngine.Debug.LogFormat("define class {0}: {1}", type, protoVal);
            JSApi.JS_FreeValue(ctx, protoVal);
            return decl;
        }

        public ClassDecl CreateEnum(string typename, Type type)
        {
            return CreateClass(typename, type, JSApi.class_private_ctor);
        }
    }
}