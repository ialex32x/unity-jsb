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

        public DelegateWrapperCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("[{0}]", typeof(JSBindingAttribute).Name);
            // this.cg.cs.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.cs.AppendLine("public partial class {0} : {1}", CodeGenerator.NameOfDelegates, typeof(Binding.Values).Name);
            this.cg.cs.AppendLine("{");
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
                    }
                    this.cg.cs.DecTabLevel();
                    this.cg.cs.AppendLine("}");
                }
                this.cg.cs.DecTabLevel();
                this.cg.cs.AppendLine("}");
                this.cg.cs.AppendLine("ns.Close();");
            }
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    public class DelegateCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public DelegateCodeGen(CodeGenerator cg, DelegateBridgeBindingInfo delegateBindingInfo, int index)
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
            this.cg.cs.AppendLine($"public static unsafe {returnTypeName} {delegateName}({firstArgument}{arglist})");
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
            this.cg.cs.AppendLine("var ctx = fn.ctx;");

            if (nargs > 0)
            {
                this.cg.cs.AppendLine("var argv = stackalloc JSValue[{0}];", nargs);
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

    public class HotfixDelegateCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        private string GetSignature(HotfixDelegateBindingInfo delegateBindingInfo, int index, string arglist, out string sig)
        {
            sig = "_HotfixDelegate" + index;
            var str = "public delegate ";
            str += this.cg.bindingManager.GetCSTypeFullName(delegateBindingInfo.returnType) + " ";
            str += sig + "(";
            str += arglist;
            str += ");";
            return str;
        }

        public string GetCSArglistDecl(Type self, bool isStatic, string selfName, ParameterInfo[] parameters)
        {
            var arglist = this.cg.bindingManager.GetCSArglistDecl(parameters);
            var firstArgType = isStatic ? "Type" : "object";
            var firstArg = firstArgType + " " + selfName;

            return string.IsNullOrEmpty(arglist) ? firstArg : firstArg + ", " + arglist;
        }

        public HotfixDelegateCodeGen(CodeGenerator cg, HotfixDelegateBindingInfo delegateBindingInfo, int index)
        {
            this.cg = cg;
            var self_name = "_hotfix_this";
            var js_push = delegateBindingInfo.isStatic ? "js_push_type" : "js_push_classvalue_hotfix";
            var nargs = delegateBindingInfo.parameters.Length;
            var retName = this.cg.bindingManager.GetUniqueName(delegateBindingInfo.parameters, "ret");
            var firstArgument = typeof(ScriptDelegate) + " fn";
            var returnTypeName = this.cg.bindingManager.GetCSTypeFullName(delegateBindingInfo.returnType);
            var delegateName = CodeGenerator.NameOfHotfixDelegates + index;
            var arglist = GetCSArglistDecl(delegateBindingInfo.thisType, delegateBindingInfo.isStatic, self_name, delegateBindingInfo.parameters);
            string sig;
            var delegateSig = GetSignature(delegateBindingInfo, index, arglist, out sig);

            this.cg.cs.AppendLine(delegateSig);
            this.cg.cs.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.cs.AppendLine("[{0}(typeof({1}))]",
                this.cg.bindingManager.GetCSTypeFullName(typeof(JSDelegateAttribute)),
                sig);
            this.cg.bindingManager.log.AppendLine("emitting delegate decl: {0}", sig);
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
                    var parameter = delegateBindingInfo.parameters[i];
                    var pusher = this.cg.AppendValuePusher(parameter.ParameterType, parameter.Name);
                    this.cg.cs.AppendLine("argv[{0}] = {1};", i, pusher);
                    this.cg.cs.AppendLine("if (argv[{0}].IsException())", i);
                    using (this.cg.cs.CodeBlockScope())
                    {
                        for (var j = 0; j < i; j++)
                        {
                            this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, argv[{0}]);", j);
                        }

                        this.cg.cs.AppendLine("throw new Exception(ctx.GetExceptionString());");
                    }
                }
                this.cg.cs.AppendLine("var this_obj = js_push_classvalue_hotfix(ctx, {0});", self_name);
                this.cg.cs.AppendLine("var rval = fn.Invoke(ctx, this_obj, {0}, argv);", nargs);
                this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, this_obj);");
            }
            else
            {
                this.cg.cs.AppendLine("var this_obj = js_push_classvalue_hotfix(ctx, {0});", self_name);
                this.cg.cs.AppendLine("var rval = fn.Invoke(ctx, this_obj);");
                this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, this_obj);");
            }

            if (delegateBindingInfo.returnType != typeof(void))
            {
                this.cg.cs.AppendLine($"{this.cg.bindingManager.GetCSTypeFullName(delegateBindingInfo.returnType)} {retName};");
                var getter = this.cg.bindingManager.GetScriptObjectGetter(delegateBindingInfo.returnType, "ctx", "rval", retName);
                this.cg.cs.AppendLine("var succ = {0};", getter);

                FreeArgs(nargs);
                CheckReturnValue();

                this.cg.cs.AppendLine("if (succ)");
                using (this.cg.cs.CodeBlockScope())
                {
                    this.cg.cs.AppendLine($"return {retName};");
                }
                this.cg.cs.AppendLine("else");
                using (this.cg.cs.CodeBlockScope())
                {
                    this.cg.cs.AppendLine($"throw new Exception(\"js exception caught\");");
                }
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
            using (this.cg.cs.CodeBlockScope())
            {
                this.cg.cs.AppendLine("throw new Exception(ctx.GetExceptionString());");
            }
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
