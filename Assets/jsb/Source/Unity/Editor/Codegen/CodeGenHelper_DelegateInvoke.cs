using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    //TODO: 支持 ref/out, 并替换 ref/out 实现
    public class DelegateCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        private ParameterInfo[] GetInputParameters(ParameterInfo[] parameters)
        {
            return parameters.Where(p => !p.IsOut).ToArray();
        }

        public DelegateCodeGen(CodeGenerator cg, DelegateBridgeBindingInfo delegateBindingInfo, int index)
        {
            this.cg = cg;
            var inputParameters = GetInputParameters(delegateBindingInfo.parameters);
            var nargs = inputParameters.Length;
            var retName = this.cg.bindingManager.GetUniqueName(delegateBindingInfo.parameters, "ret");
            var firstArgument = typeof(ScriptDelegate) + " fn";
            var returnTypeName = this.cg.bindingManager.GetCSTypeFullName(delegateBindingInfo.returnType);
            var delegateName = CodeGenerator.NameOfDelegates + index;
            var arglist = this.cg.bindingManager.GetCSArglistDecl(delegateBindingInfo.parameters);

            foreach (var target in delegateBindingInfo.types)
            {
                this.cg.cs.AppendLine("[{0}(typeof({1}))]",
                    this.cg.bindingManager.GetCSTypeFullName(typeof(JSDelegateAttribute)),
                    this.cg.bindingManager.GetCSTypeFullName(target));
                this.cg.bindingManager.log.AppendLine("emitting delegate decl: {0}", target);
            }
            if (!string.IsNullOrEmpty(arglist))
            {
                arglist = ", " + arglist;
            }
            this.cg.cs.AppendLine($"public static unsafe {returnTypeName} {delegateName}({firstArgument}{arglist})");
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
            this.cg.cs.AppendLine("var ctx = fn.ctx;");

            if (nargs > 0)
            {
                this.cg.cs.AppendLine("var argv = stackalloc JSValue[{0}];", nargs);
                for (var i = 0; i < nargs; i++)
                {
                    var parameter = inputParameters[i];
                    var pusher = this.cg.AppendValuePusher(parameter.ParameterType, parameter.Name);
                    this.cg.cs.AppendLine("argv[{0}] = {1};", i, pusher);
                    this.cg.cs.AppendLine("if (argv[{0}].IsException())", i);
                    this.cg.cs.AppendLine("{");
                    this.cg.cs.AddTabLevel();
                    for (var j = 0; j < i; j++)
                    {
                        this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, argv[{0}]);", j);
                    }

                    this.cg.cs.AppendLine("throw new Exception(ctx.GetExceptionString());");
                    this.cg.cs.DecTabLevel();
                    this.cg.cs.AppendLine("}");
                }
                this.cg.cs.AppendLine("var rval = fn.Invoke(ctx, {0}, argv);", nargs);
            }
            else
            {
                this.cg.cs.AppendLine("var rval = fn.Invoke(ctx);");
            }

            if (delegateBindingInfo.returnType != typeof(void))
            {
                this.cg.cs.AppendLine($"{this.cg.bindingManager.GetCSTypeFullName(delegateBindingInfo.returnType)} {retName};");
                var getter = this.cg.bindingManager.GetScriptObjectGetter(delegateBindingInfo.returnType, "ctx", "rval", retName);
                this.cg.cs.AppendLine("var succ = {0};", getter);

                FreeArgs(nargs);
                CheckReturnValue();

                this.cg.cs.AppendLine("if (succ)");
                this.cg.cs.AppendLine("{");
                this.cg.cs.AddTabLevel();
                this.cg.cs.AppendLine($"return {retName};");
                this.cg.cs.DecTabLevel();
                this.cg.cs.AppendLine("}");
                this.cg.cs.AppendLine("else");
                this.cg.cs.AppendLine("{");
                this.cg.cs.AddTabLevel();
                this.cg.cs.AppendLine($"throw new Exception(\"js exception caught\");");
                this.cg.cs.DecTabLevel();
                this.cg.cs.AppendLine("}");
            }
            else
            {
                FreeArgs(nargs);
                CheckReturnValue();
            }
        }

        private void CheckReturnValue()
        {
            this.cg.cs.AppendLine("if (rval.IsException())");
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
            this.cg.cs.AppendLine("throw new Exception(ctx.GetExceptionString());");
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
            this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, rval);");
        }

        private void FreeArgs(int nargs)
        {
            for (var i = 0; i < nargs; i++)
            {
                this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, argv[{0}]);", i);
                // this.cg.cs.AppendLine("argv[{0}] = JSApi.JS_UNDEFINED;", i);
            }
        }

        public void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }
}
