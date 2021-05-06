using System;
using System.Reflection;

namespace QuickJS.Binding
{
    public delegate void BindAction(ScriptRuntime runtime);

    public static class DefaultBinder
    {
        public static BindAction GetBinder(bool useReflectBind)
        {
            return useReflectBind ? (BindAction)ReflectBind : StaticBind;
        }

        public static void StaticBind(ScriptRuntime runtime)
        {
            var logger = runtime.GetLogger();
            var bindAll = typeof(Binding.Values).GetMethod("BindAll", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (bindAll == null)
            {
                if (logger != null)
                {
                    logger.Write(Utils.LogLevel.Error, "generate binding code before run, or turn on ReflectBind");
                }
                return;
            }

            var codeGenVersionField = typeof(Binding.Values).GetField("CodeGenVersion");
            if (codeGenVersionField == null || !codeGenVersionField.IsStatic || !codeGenVersionField.IsLiteral || codeGenVersionField.FieldType != typeof(uint))
            {
                if (logger != null)
                {
                    logger.Write(Utils.LogLevel.Error, "binding code version mismatch");
                }
                return;
            }

            var codeGenVersion = (uint)codeGenVersionField.GetValue(null);
            if (codeGenVersion != ScriptEngine.VERSION)
            {
                if (logger != null)
                {
                    logger.Write(Utils.LogLevel.Warn, "CodeGenVersion: {0} != {1}", codeGenVersion, ScriptEngine.VERSION);
                }
            }

            bindAll.Invoke(null, new object[] { runtime });
        }

        public static void ReflectBind(ScriptRuntime runtime)
        {
            var logger = runtime.GetLogger();
            try
            {
                var UnityHelper = Binding.Values.FindType("QuickJS.Unity.UnityHelper");
                if (UnityHelper != null)
                {
                    var IsReflectBindingSupported = UnityHelper.GetMethod("IsReflectBindingSupported");
                    if (IsReflectBindingSupported != null && (bool)IsReflectBindingSupported.Invoke(null, null))
                    {
                        var bindAll = UnityHelper.GetMethod("InvokeReflectBinding");
                        if (bindAll != null)
                        {
                            bindAll.Invoke(null, new object[] { runtime });
                            return;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            if (logger != null)
            {
                logger.Write(Utils.LogLevel.Error, "failed to get method: UnityHelper.InvokeReflectBinding");
            }
        }
    }
}
