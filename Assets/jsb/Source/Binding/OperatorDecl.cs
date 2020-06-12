using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public struct OperatorDecl
    {
        public JSValue proto;
        public JSValue[] operators;

        public OperatorDecl(JSValue proto, JSValue[] operators)
        {
            this.proto = proto;
            this.operators = operators;
        }
        
        public void Register(JSContext ctx, JSValue create)
        {
            if (!create.IsUndefined())
            {
                unsafe
                {
                    fixed (JSValue* ptr = operators)
                    {
                        var rval = JSApi.JS_Call(ctx, create, JSApi.JS_UNDEFINED, operators.Length, ptr);
                        if (rval.IsException())
                        {
                            ctx.print_exception();
                        }
                        else
                        {
                            JSApi.JS_DefinePropertyValue(ctx, proto, JSApi.JS_ATOM_Symbol_operatorSet, rval, JSPropFlags.DEFAULT);
                        }
                    }
                }
            }

            JSApi.JS_FreeValue(ctx, proto);
            for (int i = 0, len = operators.Length; i < len; i++)
            {
                JSApi.JS_FreeValue(ctx, operators[i]);
            }
        }
    }
}