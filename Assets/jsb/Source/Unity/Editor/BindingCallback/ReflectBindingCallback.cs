using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    //TODO: (未完成) 不导出绑定代码的情况下, 注册反射绑定
    public class ReflectBindingCallback : IBindingCallback
    {
        private ScriptRuntime _runtime;
        private Module.ProxyModuleRegister _moduleReg;

        public ReflectBindingCallback(ScriptRuntime runtime)
        {
            _runtime = runtime;

            ReflectBindValueOp.Register<string>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<bool>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<char>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<byte>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<sbyte>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<double>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<float>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<short>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<ushort>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<int>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<uint>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<long>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            ReflectBindValueOp.Register<ulong>(Binding.Values.js_push_primitive, Binding.Values.js_get_primitive);
            
            ReflectBindValueOp.Register<DateTime>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Vector2>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Vector2Int>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Vector3>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Vector3Int>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Vector4>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Rect>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Quaternion>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<LayerMask>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Ray>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Color>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Color32>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<Matrix4x4>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
        }

        public void BeginStaticModule(string moduleName)
        {
            _moduleReg = new Module.ProxyModuleRegister(_runtime);
        }

        public void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo, string[] elements, string jsName)
        {
            var ns = CodeGenUtils.NormalizeEx(elements, jsName);
            var type = typeBindingInfo.type;

            _runtime.AddTypeReference(_moduleReg, type, register => typeBindingInfo.DoReflectBind(register), ns);
        }

        public void EndStaticModule(string moduleName)
        {
            _runtime.AddStaticModule(moduleName, _moduleReg);
        }

        public void OnPreGenerateDelegate(DelegateBridgeBindingInfo bindingInfo)
        {
            var typeDB = _runtime.GetTypeDB();
            var method = bindingInfo.reflect;

            foreach (var delegateType in bindingInfo.types)
            {
                typeDB.AddDelegate(delegateType, method);
            }

        }

        public void OnPostGenerateDelegate(DelegateBridgeBindingInfo bindingInfo)
        {
        }

        public bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total)
        {
            return false;
        }

        public void OnGenerateFinish()
        {
        }
    }
}
