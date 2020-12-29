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
        }

        public void BeginStaticModule(string moduleName)
        {
            _moduleReg = new Module.ProxyModuleRegister(_runtime);
        }

        public void AddTypeReference(string moduleName, Type type, string[] elements, string jsName)
        {
            var typeDB = _runtime.GetTypeDB();
            var ns = CodeGenUtils.NormalizeEx(elements, jsName);
            _runtime.AddTypeReference(_moduleReg, type, typeDB.GetDynamicTypeBind(type), ns);
        }

        public void EndStaticModule(string moduleName)
        {
            _runtime.AddStaticModule(moduleName, _moduleReg);
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
