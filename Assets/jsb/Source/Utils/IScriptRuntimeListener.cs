using System.Collections;
using QuickJS.Binding;

namespace QuickJS.Utils
{
    public interface IScriptRuntimeListener
    {
        void OnBind(ScriptRuntime runtime, TypeRegister register);
        void OnComplete(ScriptRuntime runtime);
    }
}