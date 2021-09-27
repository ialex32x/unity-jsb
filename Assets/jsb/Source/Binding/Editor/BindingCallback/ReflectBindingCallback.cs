using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public class ReflectBindingCallback : IBindingCallback
    {
        private ScriptRuntime _runtime;
        private BindingManager _bindingManager;
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

#if !JSB_UNITYLESS
            ReflectBindValueOp.Register<UnityEngine.Vector2>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Vector2Int>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Vector3>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Vector3Int>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Vector4>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Rect>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Quaternion>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.LayerMask>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Ray>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Color>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Color32>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
            ReflectBindValueOp.Register<UnityEngine.Matrix4x4>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
#endif
        }

        public void OnBindingBegin(BindingManager bindingManager)
        {
            _bindingManager = bindingManager;
        }

        public void OnBindingEnd()
        {
        }

        public void BeginStaticModule(string moduleName, int capacity)
        {
            _moduleReg = _runtime.AddStaticModuleProxy(moduleName);
        }

        public void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo)
        {
            _runtime.AddTypeReference(_moduleReg, typeBindingInfo.type, register => typeBindingInfo.DoReflectBind(register, _moduleReg), typeBindingInfo.preload, typeBindingInfo.tsTypeNaming.jsFullNameForReflectBind);
        }

        public void EndStaticModule(string moduleName)
        {
        }

        public void AddDelegate(DelegateBridgeBindingInfo bindingInfo)
        {
            var typeDB = _runtime.GetTypeDB();

            foreach (var delegateType in bindingInfo.types)
            {
                if (!typeDB.ContainsDelegate(delegateType))
                {
                    var method = _bindingManager.GetReflectedDelegateMethod(bindingInfo.returnType, bindingInfo.parameters);
                    if (method != null)
                    {
                        typeDB.AddDelegate(delegateType, method);
                    }
                }
            }
        }
    }
}
