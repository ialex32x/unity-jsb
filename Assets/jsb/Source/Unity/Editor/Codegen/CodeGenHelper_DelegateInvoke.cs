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

    public class DelegateCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected DelegateBridgeBindingInfo delegateBindingInfo;

        private ParameterInfo[] GetInputParameters(ParameterInfo[] parameters)
        {
            // return parameters.Where(p => !p.IsOut).ToArray();
            return parameters;
        }

        public DelegateCodeGen(CodeGenerator cg, DelegateBridgeBindingInfo delegateBindingInfo, int index)
        {
            this.cg = cg;
            this.delegateBindingInfo = delegateBindingInfo;
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

            _WriteBackParameters(inputParameters);

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

        // 回填 ref/out 参数
        protected void _WriteBackParameters(ParameterInfo[] parameters)
        {
            //TODO: 支持 ref/out, 并替换 ref/out 实现, 反向回填, 从 jsvalue 取值并回写给ref/out参数
            // var pIndex = 0;
            // var oIndex = 0;
            // var pBase = pIndex;
            // var needContext = true;
            // for (; pIndex < parameters.Length; pIndex++)
            // {
            //     var parameter = parameters[pIndex];
            //     var pType = parameter.ParameterType;

            //     if (!pType.IsByRef
            //      || pType == typeof(Native.JSContext) || pType == typeof(Native.JSRuntime)
            //      || pType == typeof(ScriptContext) || pType == typeof(ScriptRuntime))
            //     {
            //         continue;
            //     }

            //     var baseIndex = pIndex - pBase;

            //     this.cg.WriteParameterGetter(parameter, pIndex, false, parameter.Name, true, null);
            //     // var pusher = cg.AppendValuePusher(parameter.ParameterType, $"arg{baseIndex}");

            //     // cg.cs.AppendLine("var out{0} = {1};", oIndex, pusher);
            //     // cg.cs.AppendLine("{0} = {1};", parameter.Name, );
            //     // cg.cs.AppendLine("if (JSApi.JS_IsException(out{0}))", oIndex);
            //     // using (cg.cs.CodeBlockScope())
            //     // {
            //     //     // for (var j = 0; j < oIndex; j++)
            //     //     // {
            //     //     //     cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, out{0});", j);
            //     //     // }
            //     //     OnBeforeExceptionReturn();
            //     //     cg.cs.AppendLine("return out{0};", oIndex);
            //     // }

            //     // if (needContext)
            //     // {
            //     //     cg.cs.AppendLine("var context = ScriptEngine.GetContext(ctx);");
            //     //     needContext = false;
            //     // }

            //     // cg.cs.AppendLine("JSApi.JS_SetProperty(ctx, argv[{0}], context.GetAtom(\"value\"), out{1});", baseIndex, oIndex);
            //     oIndex++;
            // }
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
