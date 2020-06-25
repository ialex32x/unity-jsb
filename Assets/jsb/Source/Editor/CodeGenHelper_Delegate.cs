using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class DelegateWrapperCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected DelegateBindingInfo[] delegateBindingInfos;

        public DelegateWrapperCodeGen(CodeGenerator cg, DelegateBindingInfo[] delegateBindingInfos)
        {
            this.cg = cg;
            this.delegateBindingInfos = delegateBindingInfos;
            this.cg.cs.AppendLine("[{0}({1})]", typeof(JSBindingAttribute).Name, ScriptEngine.VERSION);
            // this.cg.cs.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.cs.AppendLine("public class {0} : {1} {{", CodeGenerator.NameOfDelegates, typeof(Binding.Values).Name);
            this.cg.cs.AddTabLevel();
        }

        public void Dispose()
        {
            using (new RegFuncCodeGen(cg))
            {
                this.cg.cs.AppendLine("var type = typeof({0});", CodeGenerator.NameOfDelegates);
                this.cg.cs.AppendLine("var typeDB = register.GetTypeDB();");
                this.cg.cs.AppendLine("var methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);");
                this.cg.cs.AppendLine("var ns = register.CreateNamespace(\"QuickJS\");");
                this.cg.cs.AppendLine("for (int i = 0, size = methods.Length; i < size; i++)");
                this.cg.cs.AppendLine("{");
                {
                    this.cg.cs.AddTabLevel();
                    this.cg.cs.AppendLine("var method = methods[i];");
                    this.cg.cs.AppendLine("var attributes = method.GetCustomAttributes(typeof(JSDelegateAttribute), false);");
                    this.cg.cs.AppendLine("var attributesLength = attributes.Length;");
                    this.cg.cs.AppendLine("if (attributesLength > 0)");
                    this.cg.cs.AppendLine("{");
                    this.cg.cs.AddTabLevel();
                    {
                        this.cg.cs.AppendLine("for (var a = 0; a < attributesLength; a++)");
                        this.cg.cs.AppendLine("{");
                        this.cg.cs.AddTabLevel();
                        {
                            this.cg.cs.AppendLine("var attribute = attributes[a] as JSDelegateAttribute;");
                            this.cg.cs.AppendLine("typeDB.AddDelegate(attribute.target, method);");
                        }
                        this.cg.cs.DecTabLevel();
                        this.cg.cs.AppendLine("}");

                        this.cg.cs.AppendLine("var name = \"Delegate\" + (method.GetParameters().Length - 1);");
                        this.cg.cs.AppendLine("ns.Copy(\"Dispatcher\", name);");
                        // this.cg.cs.AppendLine("if (!DuktapeDLL.duk_get_prop_string(ctx, -1, name))");
                        // this.cg.cs.AppendLine("{");
                        // this.cg.cs.AddTabLevel();
                        // {
                        //     this.cg.cs.AppendLine("DuktapeDLL.duk_get_prop_string(ctx, -2, \"Dispatcher\");");
                        //     this.cg.cs.AppendLine("DuktapeDLL.duk_put_prop_string(ctx, -3, name);");
                        // }
                        // this.cg.cs.DecTabLevel();
                        // this.cg.cs.AppendLine("}");
                        // this.cg.cs.AppendLine("DuktapeDLL.duk_pop(ctx);");
                    }
                    this.cg.cs.DecTabLevel();
                    this.cg.cs.AppendLine("}");
                }
                this.cg.cs.DecTabLevel();
                this.cg.cs.AppendLine("}");
                this.cg.cs.AppendLine("ns.Close();");


                // for (var i = 0; i < delegateBindingInfos.Length; i++)
                // {
                //     var delegateBindingInfo = delegateBindingInfos[i];
                // }
            }
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    public class DelegateCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        public DelegateCodeGen(CodeGenerator cg, DelegateBindingInfo delegateBindingInfo, int index)
        {
            this.cg = cg;
            var nargs = delegateBindingInfo.parameters.Length;
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
            this.cg.cs.AppendLine($"public static {returnTypeName} {delegateName}({firstArgument}{arglist}) {{");
            this.cg.cs.AddTabLevel();
            this.cg.cs.AppendLine("var ctx = fn.ctx;");

            if (nargs > 0)
            {
                this.cg.cs.AppendLine("var argv = new JSValue[{0}];", nargs);
                for (var i = 0; i < nargs; i++)
                {
                    var parameter = delegateBindingInfo.parameters[i];
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
