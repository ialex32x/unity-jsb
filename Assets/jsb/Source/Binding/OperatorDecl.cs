using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace QuickJS.Binding
{
    public class OperatorDecl
    {
        private Type type;
        private List<OperatorDef> self;
        private List<CrossOperatorDef> left;
        private List<CrossOperatorDef> right;

        private int _count;

        public int count { get { return _count; } }

        public OperatorDecl(Type type)
        {
            this.type = type;
            self = new List<OperatorDef>();
            left = new List<CrossOperatorDef>();
            right = new List<CrossOperatorDef>();
            _count = 1;
        }

        public void AddOperator(string op, JSCFunction func, int length)
        {
            self.Add(new OperatorDef(op, func, length));
        }

        public void AddCrossOperator(string op, JSCFunction func, int length, bool bLeft, Type sideType)
        {
            var list = bLeft ? left : right;
            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                if (list[i].type == sideType)
                {
                    list[i].operators.Add(new OperatorDef(op, func, length));
                    return;
                }
            }
            var newCrossDef = new CrossOperatorDef(sideType);
            newCrossDef.operators.Add(new OperatorDef(op, func, length));
            list.Add(newCrossDef);
            _count++;
        }

        public void Register(TypeRegister register, JSContext ctx, JSValue create)
        {
            if (!create.IsUndefined())
            {
                unsafe
                {
                    var proto = register.FindPrototype(type);
                    var argv = new JSValue[_count];

                    argv[0] = JSApi.JS_NewObject(ctx);
                    for (int i = 0, len = self.Count; i < len; i++)
                    {
                        var def = self[i];
                        var funcVal = JSApi.JS_NewCFunction(ctx, def.func, def.op, def.length);
                        JSApi.JS_DefinePropertyValue(ctx, argv[0], register.GetAtom(def.op), funcVal, JSPropFlags.DEFAULT);
                        // Debug.LogFormat("{0} operator {1}", type, def.op);
                    }

                    for (int i = 0, len = left.Count; i < len; i++)
                    {
                        var cross = left[i];
                        var sideCtor = register.GetConstructor(cross.type);
                        var operator_ = JSApi.JS_NewObject(ctx);
                        var side = "left";
                        JSApi.JS_SetProperty(ctx, operator_, register.GetAtom(side), sideCtor);
                        for (int opIndex = 0, opCount = cross.operators.Count; opIndex < opCount; opIndex++)
                        {
                            var def = cross.operators[opIndex];
                            var funcVal = JSApi.JS_NewCFunction(ctx, def.func, def.op, def.length);
                            JSApi.JS_DefinePropertyValue(ctx, operator_, register.GetAtom(def.op), funcVal, JSPropFlags.DEFAULT);
                            argv[i + 1] = operator_;
                            // Debug.LogFormat("{0} {1} operator {2} {3} ({4})", type, side, def.op, cross.type, sideCtor);
                        }
                    }

                    for (int i = 0, len = right.Count; i < len; i++)
                    {
                        var cross = right[i];
                        var sideCtor = register.GetConstructor(cross.type);
                        var operator_ = JSApi.JS_NewObject(ctx);
                        var side = "right";
                        JSApi.JS_SetProperty(ctx, operator_, register.GetAtom(side), sideCtor);
                        for (int opIndex = 0, opCount = cross.operators.Count; opIndex < opCount; opIndex++)
                        {
                            var def = cross.operators[opIndex];
                            var funcVal = JSApi.JS_NewCFunction(ctx, def.func, def.op, def.length);
                            JSApi.JS_DefinePropertyValue(ctx, operator_, register.GetAtom(def.op), funcVal, JSPropFlags.DEFAULT);
                            argv[i + 1 + left.Count] = operator_;
                            // Debug.LogFormat("{0} {1} operator {2} {3} ({4})", type, side, def.op, cross.type, sideCtor);
                        }
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
        }
    }
}