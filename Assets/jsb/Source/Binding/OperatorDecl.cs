using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace QuickJS.Binding
{
    public struct OperatorDecl
    {
        public Type type;
        public JSValue proto;
        public List<SelfOperatorDef> selfOperators;
        public List<OperatorDef> operatorDefs;

        public OperatorDecl(Type type, JSValue proto, List<SelfOperatorDef> selfOperators, List<OperatorDef> operators)
        {
            this.type = type;
            this.proto = proto;
            this.selfOperators = selfOperators;
            this.operatorDefs = operators;
        }

        public void Register(TypeRegister register, JSContext ctx, JSValue create)
        {
            if (!create.IsUndefined())
            {
                unsafe
                {
                    var argv = new JSValue[1 + operatorDefs.Count];

                    argv[0] = JSApi.JS_NewObject(ctx);
                    for (int i = 0, len = selfOperators.Count; i < len; i++)
                    {
                        var def = selfOperators[i];
                        var funcVal = JSApi.JS_NewCFunction(ctx, def.func, def.op, def.length);
                        JSApi.JS_DefinePropertyValue(ctx, argv[0], register.GetAtom(def.op), funcVal, JSPropFlags.DEFAULT);
                        // Debug.LogFormat("{0} operator {1}", type, def.op);
                    }
                    
                    for (int i = 0, len = operatorDefs.Count; i < len; i++)
                    {
                        var def = operatorDefs[i];
                        var sideProto = register.FindPrototypeOf(def.type);
                        var operator_ = JSApi.JS_NewObject(ctx);
                        JSApi.JS_SetProperty(ctx, operator_, register.GetAtom(def.side), JSApi.JS_DupValue(ctx, sideProto));
                        var funcVal = JSApi.JS_NewCFunction(ctx, def.func, def.op, def.length);
                        JSApi.JS_DefinePropertyValue(ctx, operator_, register.GetAtom(def.op), funcVal, JSPropFlags.DEFAULT);
                        argv[i + 1] = operator_;
                        // Debug.LogFormat("{0} l/r operator {1} {2} ({3})", type, def.op, def.type, sideProto);
                    }

                    fixed (JSValue* ptr = argv)
                    {
                        var rval = JSApi.JS_Call(ctx, create, JSApi.JS_UNDEFINED, argv.Length, ptr);
                        if (rval.IsException())
                        {
                            ctx.print_exception(LogLevel.Warn, string.Format("[{0} operators failed]", type));
                        }
                        else
                        {
                            JSApi.JS_DefinePropertyValue(ctx, proto, JSApi.JS_ATOM_Symbol_operatorSet, rval, JSPropFlags.DEFAULT);
                        }
                    }

                    for (int i = 0, len = argv.Length; i < len; i++)
                    {
                        JSApi.JS_FreeValue(ctx, argv[i]);
                    }
                }
            }

            JSApi.JS_FreeValue(ctx, proto);
        }
    }
}