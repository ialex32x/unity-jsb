using System;
using System.Reflection;

namespace QuickJS.Binding
{
    public delegate void BindAction(ScriptRuntime runtime);

    public static class DefaultBinder
    {
        /// <summary>
        /// determine which type binding mode to use (between staticbind and reflectbind)
        /// </summary>
        public static BindAction GetBinder(bool useReflectBind)
        {
            return useReflectBind && IsReflectBindingSupported() ? (BindAction)ReflectBind : StaticBind;
        }

        public static BindAction GetBinder(string bindingMethod)
        {
            switch (bindingMethod.ToLower().Replace(" ", "").Replace("-", ""))
            {
                case "reflectbind": return ReflectBind;
                case "inmemorybind": return InMemoryBind;
                default: return StaticBind;
            }
        }

        public static bool IsStaticBinding(BindAction bindAction)
        {
            return bindAction == StaticBind;
        }

        public static BindAction StaticBind = (runtime) =>
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var typeName = $"{Values.NamespaceOfStaticBinder}.{Values.ClassNameOfStaticBinder}";
            Type type = null;
            for (var i = assemblies.Length - 1; i >= 0; --i)
            {
                var assembly = assemblies[i];
                if (!assembly.IsDynamic)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            _StaticBindInternal(runtime, type);
        };

        public static void _StaticBindInternal(ScriptRuntime runtime, Type type)
        {
            var bindAll = type?.GetMethod(Values.MethodNameOfStaticBinder, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (bindAll == null)
            {
                Diagnostics.Logger.Binding.Error("generate binding code before run, or turn on ReflectBind");
                return;
            }

            var codeGenVersionField = type.GetField("CodeGenVersion");
            if (codeGenVersionField == null || !codeGenVersionField.IsStatic || !codeGenVersionField.IsLiteral || codeGenVersionField.FieldType != typeof(uint))
            {
                Diagnostics.Logger.Binding.Error("binding code version mismatch");
                return;
            }

            var codeGenVersion = (uint)codeGenVersionField.GetValue(null);
            if (codeGenVersion != ScriptEngine.VERSION)
            {
                Diagnostics.Logger.Binding.Warning("CodeGenVersion: {0} != {1}", codeGenVersion, ScriptEngine.VERSION);
            }

            bindAll.Invoke(null, new object[] { runtime });
        }

        public static bool IsReflectBindingSupported()
        {
            try
            {
                var UnityHelper = Binding.Values.FindType("QuickJS.Unity.UnityHelper");
                if (UnityHelper != null)
                {
                    var IsReflectBindingSupported = UnityHelper.GetMethod("IsReflectBindingSupported");
                    if (IsReflectBindingSupported != null && (bool)IsReflectBindingSupported.Invoke(null, null))
                    {
                        var InvokeReflectBinding = UnityHelper.GetMethod("InvokeReflectBinding");
                        return InvokeReflectBinding != null;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void ReflectBind(ScriptRuntime runtime)
        {
            try
            {
                var typeName = "QuickJS.Unity.UnityHelper";
                var methodName = "InvokeReflectBinding";
                var UnityHelper = Binding.Values.FindType(typeName);
                if (UnityHelper != null)
                {
                    var IsReflectBindingSupported = UnityHelper.GetMethod("IsReflectBindingSupported");
                    if (IsReflectBindingSupported != null && (bool)IsReflectBindingSupported.Invoke(null, null))
                    {
                        var InvokeReflectBinding = UnityHelper.GetMethod(methodName);
                        if (InvokeReflectBinding != null)
                        {
                            InvokeReflectBinding.Invoke(null, new object[] { runtime });
                            return;
                        }
                    }
                }
                throw new Exception($"failed to invoke {typeName}.{methodName}");
            }
            catch (Exception exception)
            {
                Diagnostics.Logger.Binding.Exception("fallback to StaticBind mode", exception);
                StaticBind(runtime);
            }
        }

        public static void InMemoryBind(ScriptRuntime runtime)
        {
            try
            {
                var typeName = "QuickJS.Unity.UnityHelper";
                var methodName = "InvokeInMemoryBinding";
                var UnityHelper = Binding.Values.FindType(typeName);
                if (UnityHelper != null)
                {
                    var IsInMemoryBindingSupported = UnityHelper.GetMethod("IsInMemoryBindingSupported");
                    if (IsInMemoryBindingSupported != null && (bool)IsInMemoryBindingSupported.Invoke(null, null))
                    {
                        var InvokeInMemoryBinding = UnityHelper.GetMethod(methodName);
                        if (InvokeInMemoryBinding != null)
                        {
                            InvokeInMemoryBinding.Invoke(null, new object[] { runtime });
                            return;
                        }
                    }
                }
                throw new Exception($"failed to invoke {typeName}.{methodName}");
            }
            catch (Exception exception)
            {
                Diagnostics.Logger.Binding.Exception("fallback to ReflectBind mode", exception);
                ReflectBind(runtime);
            }
        }
    }
}
