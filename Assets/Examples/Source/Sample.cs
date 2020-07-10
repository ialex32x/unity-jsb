using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;

namespace jsb
{
    using UnityEngine;

    public class Sample : MonoBehaviour, IScriptRuntimeListener
    {
        public bool useResources;
        public bool sourceMap;
        private ScriptRuntime _rt;

        void Awake()
        {
            IFileSystem fileSystem;

            _rt = ScriptEngine.CreateRuntime();
            _rt.AddSearchPath("node_modules");

            if (useResources)
            {
                fileSystem = new ResourcesFileSystem();
                _rt.AddSearchPath("dist");
            }
            else
            {
                fileSystem = new DefaultFileSystem();
                _rt.AddSearchPath("Assets/Examples/Scripts/out");
            }

            _rt.EnableStacktrace();
            if (sourceMap)
            {
                _rt.EnableSourceMap();
            }
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
            WebSockets.WebSocket.Bind(register);
        }

        public void OnComplete(ScriptRuntime runtime)
        {
            _rt.EvalMain("main.js");
        }
    }
}