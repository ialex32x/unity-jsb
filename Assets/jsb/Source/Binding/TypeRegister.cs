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

        private List<Type> _pendingTypes = new List<Type>();
        private List<OperatorDecl> _operatorDecls = new List<OperatorDecl>();
        private Dictionary<Type, int> _operatorDeclIndex = new Dictionary<Type, int>();

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
            var ctx = (JSContext)context;

            _context = context;
            _atoms = new AtomCache(_context);
            _db = runtime.GetTypeDB();
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
            return new NamespaceDecl(this, _AutoProperty(_AutoProperty(el1), el2));
        }

        public NamespaceDecl CreateNamespace(string el1, string el2, string el3) // [parent]
        {
            return new NamespaceDecl(this, _AutoProperty(_AutoProperty(_AutoProperty(el1), el2), el3));
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

        public ClassDecl CreateClass(string typename, Type type, JSCFunctionMagic ctorFunc)
        {
            return CreateClass(JSApi.JS_UNDEFINED, typename, type, ctorFunc);
        }

        public ClassDecl CreateClass(JSValue nsValue, string typename, Type type, JSCFunctionMagic ctorFunc)
        {
            var nameAtom = GetAtom(typename);
            JSContext ctx = _context;
            var protoVal = JSApi.JS_NewObject(ctx);
            var type_id = RegisterType(type, protoVal);
            var ctorVal = JSApi.JSB_NewCFunctionMagic(ctx, ctorFunc, nameAtom, 0, JSCFunctionEnum.JS_CFUNC_constructor_magic, type_id);
            var decl = new ClassDecl(this, ctorVal, protoVal, type);
            JSApi.JS_SetConstructor(ctx, ctorVal, protoVal);
            JSApi.JSB_SetBridgeType(ctx, ctorVal, type_id);
            if (!nsValue.IsNullish())
            {
                JSApi.JS_DefinePropertyValue(ctx, nsValue, nameAtom, ctorVal, JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            }
            else
            {
                JSApi.JS_FreeValue(ctx, ctorVal);
            }
            // UnityEngine.Debug.LogFormat("define class {0}: {1}", type, protoVal);
            JSApi.JS_FreeValue(ctx, protoVal);
            return decl;
        }

        public ClassDecl CreateClass(string typename, Type type, IDynamicMethod dynamicMethod)
        {
            return CreateClass(JSApi.JS_UNDEFINED, typename, type, dynamicMethod);
        }

        public ClassDecl CreateClass(JSValue nsValue, string typename, Type type, IDynamicMethod dynamicMethod)
        {
            var nameAtom = GetAtom(typename);
            JSContext ctx = _context;
            var protoVal = JSApi.JS_NewObject(ctx);
            var type_id = RegisterType(type, protoVal);
            var ctorVal = _db.NewDynamicMethod(nameAtom, dynamicMethod);
            var decl = new ClassDecl(this, ctorVal, protoVal, type);
            JSApi.JS_SetConstructor(ctx, ctorVal, protoVal);
            JSApi.JSB_SetBridgeType(ctx, ctorVal, type_id);
            if (!nsValue.IsNullish())
            {
                JSApi.JS_DefinePropertyValue(ctx, nsValue, nameAtom, ctorVal, JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            }
            else
            {
                JSApi.JS_FreeValue(ctx, ctorVal);
            }
            // UnityEngine.Debug.LogFormat("define class {0}: {1}", type, protoVal);
            JSApi.JS_FreeValue(ctx, protoVal);
            return decl;
        }

        // return type id, 不可重复注册
        public int RegisterType(Type type, JSValue proto)
        {
            _pendingTypes.Add(type);
            return _db.AddType(type, proto);
        }

        public int RegisterType(Type type)
        {
            return _db.AddType(type, JSApi.JS_UNDEFINED);
        }

        private void SubmitOperators()
        {
            // 提交运算符重载
            var ctx = (JSContext)_context;
            var operatorCreate = _context.GetOperatorCreate();

            if (!operatorCreate.IsUndefined())
            {
                var count = _operatorDecls.Count;
                for (var i = 0; i < count; i++)
                {
                    _operatorDecls[i].Register(this, ctx, operatorCreate);
                }
            }

            JSApi.JS_FreeValue(ctx, operatorCreate);
            _operatorDeclIndex.Clear();
            _operatorDecls.Clear();
        }

        private OperatorDecl GetOperatorDecl(Type type, out int index)
        {
            if (_operatorDeclIndex.TryGetValue(type, out index))
            {
                return _operatorDecls[index];
            }
            var decl = new OperatorDecl(type);
            index = _operatorDecls.Count;
            _operatorDeclIndex[type] = index;
            _operatorDecls.Add(decl);
            return decl;
        }

        public JSValue GetConstructor(Type type)
        {
            if (type == typeof(string) || type == typeof(char))
            {
                return _context.GetStringConstructor();
            }

            if (type.IsValueType && (type.IsPrimitive || type.IsEnum))
            {
                return _context.GetNumberConstructor();
            }

            var val = _db.FindPrototypeOf(type);
            return JSApi.JS_GetProperty(_context, val, JSApi.JS_ATOM_constructor);
        }

        public JSValue FindPrototype(Type type)
        {
            var val = _db.FindPrototypeOf(type);
            return val;
        }

        // self operator for type
        public void RegisterOperator(Type type, string op, JSCFunction func, int length)
        {
            int index;
            var decl = GetOperatorDecl(type, out index);
            decl.AddOperator(op, func, length);
        }

        // left/right operator for type
        public void RegisterOperator(Type type, string op, JSCFunction func, int length, bool left, Type sideType)
        {
            if (sideType == typeof(string) || (sideType.IsValueType && (sideType.IsPrimitive || sideType.IsEnum)))
            {
                int index;
                var decl = GetOperatorDecl(type, out index);
                decl.AddCrossOperator(op, func, length, left, sideType);
            }
            else
            {
                int index1, index2;
                var decl1 = GetOperatorDecl(type, out index1);
                var decl2 = GetOperatorDecl(sideType, out index2);
                if (index2 > index1)
                {
                    decl2.AddCrossOperator(op, func, length, !left, type);
                }
                else
                {
                    decl1.AddCrossOperator(op, func, length, left, sideType);
                }
            }
        }

        public void Finish()
        {
            SubmitOperators();
            _atoms.Clear();
            var ctx = (JSContext)_context;
            for (int i = 0, count = _pendingTypes.Count; i < count; i++)
            {
                var type = _pendingTypes[i];
                var proto = _db.GetPropertyOf(type);
                if (!proto.IsNullish())
                {
                    var baseType = type.BaseType;
                    var parentProto = _db.FindPrototypeOf(baseType);
                    if (!parentProto.IsNullish())
                    {
                        JSApi.JS_SetPrototype(ctx, proto, parentProto);
                    }
                }
            }
            _pendingTypes.Clear();
        }
    }
}