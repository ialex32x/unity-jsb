using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class TopLevelCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public TopLevelCodeGen(CodeGenerator cg, TypeBindingInfo type)
        {
            this.cg = cg;
            this.AppendCommonHead();
            this.cg.cs.AppendLine("// Assembly: {0}", type.Assembly.GetName());
            this.cg.cs.AppendLine("// Type: {0}", type.FullName);
            this.AppendCommon();

            // this.cg.typescript.AppendLine("// {0} {1}", Environment.UserName, this.cg.bindingManager.dateTime);
        }

        public TopLevelCodeGen(CodeGenerator cg, string name)
        {
            this.cg = cg;
            this.AppendCommonHead();
            this.cg.cs.AppendLine("// Special: {0}", name);
            this.AppendCommon();
        }

        private void AppendCommonHead()
        {
            if (cg.bindingManager.prefs.debugCodegen)
            {
                this.cg.cs.AppendLine("/*");
            }
        }

        private void AppendCommonTail()
        {
            if (cg.bindingManager.prefs.debugCodegen)
            {
                this.cg.cs.AppendLine("*/");
            }
        }

        private void AppendCommon()
        {
            this.cg.cs.AppendLine("// Unity: {0}", Application.unityVersion);
            this.cg.cs.AppendLine("using System;");
            this.cg.cs.AppendLine("using System.Collections.Generic;");
            this.cg.cs.AppendLine();
        }

        public void Dispose()
        {
            AppendCommonTail();
        }
    }

    public class NamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string csNamespace;
        protected string tsNamespace;
        protected bool tsNamespaceWrite;

        public NamespaceCodeGen(CodeGenerator cg, string csNamespace)
        {
            this.cg = cg;
            this.csNamespace = csNamespace;
            if (!string.IsNullOrEmpty(csNamespace))
            {
                this.cg.cs.AppendLine("namespace {0} {{", csNamespace);
                this.cg.cs.AddTabLevel();
            }
            this.AddUsingStatements();
        }

        public NamespaceCodeGen(CodeGenerator cg, string csNamespace, string tsNamespace)
        {
            this.cg = cg;
            this.csNamespace = csNamespace;
            this.tsNamespace = tsNamespace;
            if (!string.IsNullOrEmpty(csNamespace))
            {
                this.cg.cs.AppendLine("namespace {0} {{", csNamespace);
                this.cg.cs.AddTabLevel();
            }
            this.AddUsingStatements();
            if (!string.IsNullOrEmpty(tsNamespace))
            {
                tsNamespaceWrite = true;
                this.cg.tsDeclare.AppendLine("declare namespace {0} {{", tsNamespace);
                this.cg.tsDeclare.AddTabLevel();
            }
        }

        private void AddUsingStatements()
        {
            this.cg.cs.AppendLine("using QuickJS;");
            this.cg.cs.AppendLine("using QuickJS.Binding;");
            this.cg.cs.AppendLine("using QuickJS.Native;");
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(csNamespace))
            {
                this.cg.cs.DecTabLevel();
                this.cg.cs.AppendLine("}");
            }
            if (tsNamespaceWrite)
            {
                this.cg.tsDeclare.DecTabLevel();
                this.cg.tsDeclare.AppendLine("}");
            }
        }
    }

    public class RegFuncCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public RegFuncCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("public static void Bind(TypeRegister register)");
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    public class RegFuncNamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public RegFuncNamespaceCodeGen(CodeGenerator cg, TypeBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.cg.cs.Append("var ns = register.CreateNamespace(");
            // Debug.LogErrorFormat("{0}: {1}", bindingInfo.type, bindingInfo.Namespace);
            if (bindingInfo.jsNamespace != null)
            {
                var split_ns = bindingInfo.jsNamespace.Split('.');
                var ns_count = split_ns.Length;
                for (var i = 0; i < ns_count; i++)
                {
                    var el_ns = split_ns[i];
                    if (i == ns_count - 1)
                    {
                        this.cg.cs.AppendL("\"{0}\"", el_ns);
                    }
                    else
                    {
                        this.cg.cs.AppendL("\"{0}\", ", el_ns);
                    }
                }
            }
            this.cg.cs.AppendLineL(");");
        }

        public virtual void Dispose()
        {
            this.cg.cs.AppendLine("ns.Close();");
        }
    }

    public class TypeCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected TypeBindingInfo typeBindingInfo;

        public TypeCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo)
        {
            this.cg = cg;
            this.typeBindingInfo = typeBindingInfo;
            this.cg.cs.AppendLine("[{0}({1})]", typeof(JSBindingAttribute).Name, ScriptEngine.VERSION);
            this.cg.cs.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.cs.AppendLine("public class {0} : {1} {{", typeBindingInfo.name, typeof(Binding.Values).Name);
            this.cg.cs.AddTabLevel();
        }

        public string GetTypeName(Type type)
        {
            return type.FullName.Replace(".", "_");
        }

        public virtual void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    public class PreservedCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public PreservedCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("[UnityEngine.Scripting.Preserve]");
        }

        public void Dispose()
        {
        }
    }

    public class PInvokeGuardCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public PInvokeGuardCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof({0}))]", typeof(QuickJS.Native.JSCFunction).Name);
        }

        public PInvokeGuardCodeGen(CodeGenerator cg, Type target)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof({0}))]", target.FullName);
        }

        public void Dispose()
        {
        }
    }

    // 方法绑定
    public class BindingFuncDeclareCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public BindingFuncDeclareCodeGen(CodeGenerator cg, string name)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("public static JSValue {0}(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)", name);
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    public class BindingGetterFuncDeclareCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public BindingGetterFuncDeclareCodeGen(CodeGenerator cg, string name)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("public static JSValue {0}(JSContext ctx, JSValue this_obj)", name);
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    public class BindingSetterFuncDeclareCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public BindingSetterFuncDeclareCodeGen(CodeGenerator cg, string name)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("public static JSValue {0}(JSContext ctx, JSValue this_obj, JSValue arg_val)", name);
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    // 构造方法绑定
    public class BindingConstructorDeclareCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public BindingConstructorDeclareCodeGen(CodeGenerator cg, string name)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("public static JSValue {0}(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)", name);
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }

    public class TryCatchGuradCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        public TryCatchGuradCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("try");
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
        }

        private string GetVarName(Type type)
        {
            var name = type.Name;
            return name[0].ToString().ToLower() + name.Substring(1);
        }

        private void AddCatchClause(Type exceptionType)
        {
            var varName = GetVarName(exceptionType);
            this.cg.cs.AppendLine("catch ({0} {1})", exceptionType.Name, varName);
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
            {
                this.cg.cs.AppendLine("return JSApi.ThrowException(ctx, {0});", varName);
            }
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }

        public virtual void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
            // this.AddCatchClause(typeof(NullReferenceException), "duk_reference_error");
            // this.AddCatchClause(typeof(IndexOutOfRangeException), "duk_range_error");
            this.AddCatchClause(typeof(Exception));
        }
    }

    public class PlatformCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public PlatformCodeGen(CodeGenerator cg)
        {
            this.cg = cg;

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    cg.cs.AppendLineL("#if UNITY_ANDROID");
                    break;
                case BuildTarget.iOS:
                    cg.cs.AppendLineL("#if UNITY_IOS");
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    cg.cs.AppendLineL("#if UNITY_STANDALONE_WIN");
                    break;
                case BuildTarget.StandaloneOSX:
                    cg.cs.AppendLineL("#if UNITY_STANDALONE_OSX");
                    break;
                default:
                    cg.cs.AppendLineL("#if false");
                    break;
            }
        }


        public void Dispose()
        {
            cg.cs.AppendLineL("#endif");
        }
    }
}