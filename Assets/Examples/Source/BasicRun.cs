using QuickJS;
using QuickJS.Native;
using System.Collections;
using System.Collections.Generic;

namespace Example
{
    using UnityEngine;

    public class BasicRun : MonoBehaviour
    {
        public enum RunCase
        {
            None,
            QuickJSCodebase,
            Codegen,
        }
        public RunCase runCase;

        unsafe void Start()
        {
            switch (runCase)
            {
                case RunCase.QuickJSCodebase:
                    {
                        var rt = JSApi.JS_NewRuntime();
                        JSMemoryUsage s;
                        JSApi.JS_ComputeMemoryUsage(rt, &s);
                        Debug.Log($"test {rt}: {s.malloc_count} {s.malloc_size} {s.malloc_limit}");
                        var ctx = JSApi.JS_NewContext(rt);
                        Debug.Log("test:" + ctx);

                        // // // JSApi.JS_AddIntrinsicOperators(ctx);
                        // // var obj = JSApi.JS_NewObject(ctx);
                        // // JSApi.JS_FreeValue(ctx, obj);

                        JSApi.JS_FreeContext(ctx);
                        JSApi.JS_FreeRuntime(rt);
                        Debug.Log("it's a good day");
                        GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;
                    }
                case RunCase.Codegen:
                    {
                        var options = new Dictionary<string, string>();
                        System.Reflection.MethodInfo Call = null;
                        using (var p = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("cs", options))
                        {
                            var compilerParameters = new System.CodeDom.Compiler.CompilerParameters();
                            compilerParameters.GenerateInMemory = true;
                            compilerParameters.TreatWarningsAsErrors = false;
                            compilerParameters.CompilerOptions = "-unsafe";
                            compilerParameters.ReferencedAssemblies.Add(typeof(Debug).Assembly.Location);
                            var result = p.CompileAssemblyFromSource(compilerParameters, @"
                            using UnityEngine;
                            namespace _Hidden {
                                public static class Foo {
                                    public static unsafe void Call(ref int a0) {
                                        Debug.Log(""Hello "" + a0);
                                        a0 += 1;
                                    }
                                }
                            }
                            ");
                            if (result.Errors.HasErrors)
                            {
                                foreach (var err in result.Errors)
                                {
                                    Debug.LogError(err);
                                }
                            }
                            else
                            {
                                var Foo = result.CompiledAssembly.GetType("_Hidden.Foo");
                                Call = Foo.GetMethod("Call");
                            }
                        }

                        if (Call != null)
                        {
                            var v = 99;
                            var ps = new object[] { v };
                            Call.Invoke(null, ps);
                            Debug.Log($"Call: {ps[0]}");
                        }
                        break;
                    }
            }
        }
    }
}
