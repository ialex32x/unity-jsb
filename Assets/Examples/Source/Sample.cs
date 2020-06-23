using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;

namespace jsb
{
    using UnityEngine;

    public class Sample : MonoBehaviour, IScriptRuntimeListener
    {
        private ScriptRuntime _rt;

        void Awake()
        {
            _rt = ScriptEngine.CreateRuntime();
            var fileSystem = new DefaultFileSystem();
            _rt.AddSearchPath("Assets");
            _rt.AddSearchPath("node_modules");
            _rt.EnableStacktrace();
            _rt.Initialize(fileSystem, this, new UnityLogger(), new ByteBufferPooledAllocator());
        }

        void Update()
        {
            _rt.Update(Time.deltaTime);
        }

        void OnDestroy()
        {
            _rt.Destroy();
        }

        public void OnBind(ScriptRuntime runtime, TypeRegister register)
        {
            _QuickJSBindings.Bind(register);
            WebSockets.WebSocket.Bind(register);
        }

        public void OnComplete(ScriptRuntime runtime)
        {
            _rt.EvalMain("Assets/Examples/Scripts/out/main.js");
        }
    }
}