using System.Collections;
using QuickJS.Binding;
using System.Threading.Tasks;

namespace QuickJS.Utils
{
    public interface IScriptRuntimeListener
    {
        /// <summary> 
        /// <para>Runtime 执行类型注册时回调 (Worker 也会调用此方法)</para>
        /// <para>通过 runtime.isWorker 可以判断是否是 Worker Runtime</para>
        /// </summary>
        void OnBind(ScriptRuntime runtime, TypeRegister register);

        /// <summary> 
        /// <para>Runtime 完成初始化时回调 (Worker 也会调用此方法)</para>
        /// <para>通过 runtime.isWorker 可以判断是否是 Worker Runtime</para>
        /// </summary>
        void OnComplete(ScriptRuntime runtime);
    }
}