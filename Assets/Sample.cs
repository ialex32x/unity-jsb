using System;
using System.Collections;
using System.IO;
using System.Text;
using AOT;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;

namespace jsb
{
    using UnityEngine;

    public class Sample : MonoBehaviour, IScriptRuntimeListener
    {
        private ScriptRuntime _rt;

        void Awake()
        {
            _rt = ScriptEngine.CreateRuntime();
            var fileResolver = new FileResolver(new DefaultFileSystem());
            _rt.Initialize(fileResolver, this);
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
            FooBinding.Bind(register);
            Vector3Binding.Bind(register);
        }

        public void OnComplete(ScriptRuntime runtime)
        {
            _rt.EvalSource("Assets/test.js");
        }
    }
}