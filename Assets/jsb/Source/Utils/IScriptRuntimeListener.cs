using System.Collections;
using QuickJS.Binding;
using System.Threading.Tasks;

namespace QuickJS.Utils
{
    public interface IScriptRuntimeListener
    {
        void OnBind(ScriptRuntime runtime, TypeRegister register);
        void OnComplete(ScriptRuntime runtime);
    }
}