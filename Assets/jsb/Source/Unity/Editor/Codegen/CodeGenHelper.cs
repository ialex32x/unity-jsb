using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class TopLevelCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public TopLevelCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo)
        {
            this.cg = cg;
            this.AppendCommonHead();
            this.cg.cs.AppendLine("// Assembly: {0}", typeBindingInfo.Assembly.GetName());
            this.cg.cs.AppendLine("// Type: {0}", typeBindingInfo.FullName);
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

        public NamespaceCodeGen(CodeGenerator cg, string csNamespace)
        : this(cg, csNamespace, "")
        {
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
                this.cg.tsDeclare.AppendLine($"declare namespace {this.tsNamespace} {{");
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

            if (!string.IsNullOrEmpty(tsNamespace))
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

        public RegFuncNamespaceCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo)
        {
            this.cg = cg;
            this.cg.cs.Append("var ns = register.CreateNamespace(");
            // Debug.LogErrorFormat("{0}: {1}", bindingInfo.type, bindingInfo.Namespace);
            if (!string.IsNullOrEmpty(typeBindingInfo.jsNamespace))
            {
                var split_ns = from i in typeBindingInfo.jsNamespace.Split('.') select $"\"{i}\"";
                var join_ns = string.Join(", ", split_ns);
                this.cg.cs.AppendL(join_ns);
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
            this.cg.cs.AppendLine("[{0}]", typeof(JSBindingAttribute).Name);
            // this.cg.cs.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.cs.AppendLine("public class {0} : {1}", typeBindingInfo.csBindingName, typeof(Binding.Values).Name);
            this.cg.cs.AppendLine("{");
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
            this.cg.cs.AppendLine("[MonoPInvokeCallbackAttribute(typeof({0}))]", typeof(QuickJS.Native.JSCFunction).Name);
        }

        public PInvokeGuardCodeGen(CodeGenerator cg, Type target)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("[MonoPInvokeCallbackAttribute(typeof({0}))]", target.FullName);
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

    public class EditorOnlyCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected bool isEditorOnly;

        public EditorOnlyCodeGen(CodeGenerator cg, bool isEditorOnly = true)
        {
            this.cg = cg;
            this.isEditorOnly = isEditorOnly;
            if (isEditorOnly)
            {
                cg.cs.AppendLineL("#if UNITY_EDITOR");
            }
        }


        public void Dispose()
        {
            if (isEditorOnly)
            {
                cg.cs.AppendLineL("#endif");
            }
        }
    }

    public class PlatformCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected TypeBindingFlags bf;
        protected string predef;

        public PlatformCodeGen(CodeGenerator cg, TypeBindingFlags bf)
        {
            this.cg = cg;
            this.bf = bf;
            this.predef = string.Empty;

            if ((this.bf & TypeBindingFlags.UnityRuntime) != 0)
            {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                switch (buildTarget)
                {
                    case BuildTarget.Android:
                        predef = "UNITY_ANDROID";
                        break;
                    case BuildTarget.iOS:
                        predef = "UNITY_IOS";
                        break;
                    case BuildTarget.WSAPlayer:
                        predef = "UNITY_WSA"; // not supported
                        break;
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        predef = "UNITY_STANDALONE_WIN";
                        break;
                    case BuildTarget.StandaloneOSX:
                        predef = "UNITY_STANDALONE_OSX";
                        break;
                    default:
                        predef = string.Format("false // {0} is not supported", buildTarget);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(this.predef))
            {
                cg.cs.AppendLineL("#if {0}", this.predef);
            }
        }


        public void Dispose()
        {
            if (!string.IsNullOrEmpty(predef))
            {
                cg.cs.AppendLineL("#endif");
            }
        }
    }
}