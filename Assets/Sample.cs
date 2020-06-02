using System;
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

    public class Sample : MonoBehaviour
    {
        void Awake()
        {
            var rt = ScriptEngine.CreateRuntime();
            var fileResolver = new FileResolver(new DefaultFileSystem());
            rt.Initialize(fileResolver, this);
            var register = new TypeRegister(rt.GetMainContext());
            
            Foo.Bind(register);
            register.Finish();
            // rt.EvalSource("Assets/test.js");
            rt.Destroy();
        }
    }
}
