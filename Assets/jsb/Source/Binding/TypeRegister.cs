using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using System.Reflection;

namespace QuickJS.Binding
{
    using Utils;
    using UnityEngine;

    public class TypeRegister
    {
        private ScriptContext _context;

        private TypeDB _db;

        // 注册过程中产生的 atom, 完成后自动释放 
        private AtomCache _atoms;

        private JSValue _globalObject;
        private JSValue _operatorCreate;
        private List<OperatorDecl> _operatorDecls = new List<OperatorDecl>();

        public static implicit operator JSContext(TypeRegister register)
        {
            return register._context;
        }

        public ScriptContext GetContext()
        {
            return _context;
        }

        public IScriptLogger GetLogger()
        {
            return _context.GetLogger();
        }

        public JSAtom GetAtom(string name)
        {
            return _atoms.GetAtom(name);
        }

        public TypeRegister(ScriptRuntime runtime, ScriptContext context)
        {
            _db = new TypeDB(runtime);
            _context = context;
            _atoms = new AtomCache(_context);
            var ctx = (JSContext) _context;

            _globalObject = JSApi.JS_GetGlobalObject(ctx);
            _operatorCreate = JSApi.JS_UNDEFINED;
            
            var operators = JSApi.JS_GetProperty(ctx, _globalObject, JSApi.JS_ATOM_Operators);
            if (!operators.IsNullish())
            {
                if (operators.IsException())
                {
                    ctx.print_exception();
                }
                else
                {
                    var create = JSApi.JS_GetProperty(ctx, operators, GetAtom("create"));
                    JSApi.JS_FreeValue(ctx, operators);
                    if (create.IsException())
                    {
                        ctx.print_exception();
                    }
                    else
                    {
                        if (JSApi.JS_IsFunction(ctx, create) == 1)
                        {
                            _operatorCreate = create;
                        }
                        else
                        {
                            JSApi.JS_FreeValue(ctx, create);
                        }
                    }
                }
            }

            JSApi.JS_NewClass(runtime, JSApi.JSB_GetBridgeClassID(), "CSharpClass", JSApi.class_finalizer);
        }

        public TypeDB GetTypeDB()
        {
            return _db;
        }

        // 无命名空间, 直接外围对象作为容器 (通常是global)
        public NamespaceDecl CreateNamespace() // [parent]
        {
            return new NamespaceDecl(this, _context.GetGlobalObject());
        }

        private JSValue _AutoProperty(string name)
        {
            var globalObject = _context.GetGlobalObject();
            var nameAtom = GetAtom(name);
            var ns = JSApi.JSB_NewPropertyObject(_context, globalObject, nameAtom, JSPropFlags.JS_PROP_C_W_E);
            JSApi.JS_FreeValue(_context, globalObject);
            return ns;
        }

        private JSValue _AutoProperty(JSValue thisObject, string name)
        {
            var nameAtom = GetAtom(name);
            var ns = JSApi.JSB_NewPropertyObject(_context, thisObject, nameAtom, JSPropFlags.JS_PROP_C_W_E);
            JSApi.JS_FreeValue(_context, thisObject);
            return ns;
        }

        public NamespaceDecl CreateNamespace(string el) // [parent]
        {
            return new NamespaceDecl(this, _AutoProperty(el));
        }

        public NamespaceDecl CreateNamespace(string el1, string el2) // [parent]
        {
            return new NamespaceDecl(this, _AutoProperty(_AutoProperty(el1), el1));
        }

        public NamespaceDecl CreateNamespace(string el1, string el2, string el3) // [parent]
        {
            return new NamespaceDecl(this, _AutoProperty(_AutoProperty(_AutoProperty(el1), el1), el3));
        }

        // return [parent, el]
        public NamespaceDecl CreateNamespace(params string[] els) // [parent]
        {
            var ns = _context.GetGlobalObject();
            for (int i = 0, size = els.Length; i < size; i++)
            {
                var el = els[i];
                ns = _AutoProperty(ns, el);
            }

            return new NamespaceDecl(this, ns);
        }

        // return type id
        public int RegisterType(Type type, JSValue jsValue)
        {
            return _db.AddType(type, JSApi.JS_DupValue(_context, jsValue));
        }

        public int RegisterType(Type type)
        {
            return _db.AddType(type, JSApi.JS_UNDEFINED);
        }

        private void SubmitOperators()
        {
            // 提交运算符重载
            var ctx = (JSContext) _context;
            
            for (int i = 0, count = _operatorDecls.Count; i < count; i++)
            {
                _operatorDecls[i].Register(ctx, _operatorCreate);
            }
            
            _operatorDecls.Clear();
        }

        public void AddOperatorDecl(JSValue proto, JSValue[] operators)
        {
            _operatorDecls.Add(new OperatorDecl(proto, operators));
        }

        public TypeDB Finish()
        {
            SubmitOperators();
            JSApi.JS_FreeValue(_context, _globalObject);
            JSApi.JS_FreeValue(_context, _operatorCreate);
            _atoms.Clear();
            return _db.Finish();
        }
    }
}