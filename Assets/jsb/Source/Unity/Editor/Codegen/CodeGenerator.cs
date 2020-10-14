using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace QuickJS.Unity
{
    using QuickJS.Binding;
    using UnityEngine;
    using UnityEditor;

    public partial class CodeGenerator
    {
        public const string NameOfDelegates = "_QuickJSDelegates";
        public const string NameOfHotfixDelegates = "_QuickJSHotfixDelegates";
        public const string NameOfBindingList = "_QuickJSBindings";
        public const string NamespaceOfScriptTypes = "QuickJS";
        public const string NamespaceOfInternalScriptTypes = "QuickJS.Internal";
        public const string NameOfBuffer = "ArrayBuffer";

        public BindingManager bindingManager;
        public TextGenerator cs;
        public TextGenerator tsDeclare;
        public TypeBindingFlags typeBindingFlags;

        public CodeGenerator(BindingManager bindingManager, TypeBindingFlags typeBindingFlags)
        {
            this.typeBindingFlags = typeBindingFlags;
            this.bindingManager = bindingManager;
            var tab = this.bindingManager.prefs.tab;
            var newline = this.bindingManager.prefs.newline;
            cs = new TextGenerator(newline, tab);
            tsDeclare = new TextGenerator(newline, tab);
        }

        public void Clear()
        {
            cs.Clear();
            if (!bindingManager.prefs.singleTSD)
            {
                tsDeclare.Clear();
            }
        }

        public void GenerateBindingList(List<TypeBindingInfo> orderedTypes)
        {
            this.cs.enabled = (typeBindingFlags & TypeBindingFlags.BindingCode) != 0;
            this.tsDeclare.enabled = (typeBindingFlags & TypeBindingFlags.TypeDefinition) != 0;

            using (new PlatformCodeGen(this, TypeBindingFlags.Default))
            {
                using (new TopLevelCodeGen(this, CodeGenerator.NameOfBindingList))
                {
                    using (new NamespaceCodeGen(this, typeof(Values).Namespace))
                    {
                        using (new PreservedCodeGen(this))
                        {
                            using (new PlainClassCodeGen(this, typeof(Values).Name))
                            {
                                using (new PreservedCodeGen(this))
                                {
                                    this.cs.AppendLine("public const uint CodeGenVersion = {0};", ScriptEngine.VERSION);
                                }

                                using (new PreservedCodeGen(this))
                                {
                                    using (var method = new PlainMethodCodeGen(this, "private static void BindAll(TypeRegister register)"))
                                    {
                                        var editorTypes = new List<TypeBindingInfo>();
                                        foreach (var type in orderedTypes)
                                        {
                                            if (type.genBindingCode)
                                            {
                                                if (type.isEditorRuntime)
                                                {
                                                    editorTypes.Add(type);
                                                }
                                                else
                                                {
                                                    method.AddStatement("{0}.{1}.Bind(register);", this.bindingManager.prefs.ns, type.csBindingName);
                                                }
                                            }
                                        }

                                        using (new EditorOnlyCodeGen(this))
                                        {
                                            foreach (var editorType in editorTypes)
                                            {
                                                method.AddStatement("{0}.{1}.Bind(register);", this.bindingManager.prefs.ns, editorType.csBindingName);
                                            }
                                        }
                                        method.AddStatement("{0}.{1}.Bind(register);", this.bindingManager.prefs.ns, CodeGenerator.NameOfDelegates);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // 生成委托绑定
        public void Generate(DelegateBridgeBindingInfo[] delegateBindingInfos, List<HotfixDelegateBindingInfo> exportedHotfixDelegates)
        {
            this.cs.enabled = (typeBindingFlags & TypeBindingFlags.BindingCode) != 0;
            this.tsDeclare.enabled = (typeBindingFlags & TypeBindingFlags.TypeDefinition) != 0;

            using (new PlatformCodeGen(this, TypeBindingFlags.Default))
            {
                using (new TopLevelCodeGen(this, CodeGenerator.NameOfDelegates))
                {
                    using (new NamespaceCodeGen(this, this.bindingManager.prefs.ns))
                    {
                        using (new DelegateWrapperCodeGen(this))
                        {
                            for (var i = 0; i < exportedHotfixDelegates.Count; i++)
                            {
                                var bindingInfo = exportedHotfixDelegates[i];

                                using (new PreservedCodeGen(this))
                                {
                                    using (new HotfixDelegateCodeGen(this, bindingInfo, i))
                                    {
                                    }
                                }
                            }

                            SortedList<int, int> specs = new SortedList<int, int>();
                            for (var i = 0; i < delegateBindingInfos.Length; i++)
                            {
                                var bindingInfo = delegateBindingInfos[i];
                                var nargs = bindingInfo.parameters.Length;
                                if (!specs.ContainsKey(nargs))
                                {
                                    specs.Add(nargs, 0);
                                }
                                this.bindingManager.OnPreGenerateDelegate(bindingInfo);
                                using (new EditorOnlyCodeGen(this, bindingInfo.isEditorRuntime))
                                {
                                    using (new PreservedCodeGen(this))
                                    {
                                        using (new DelegateCodeGen(this, bindingInfo, i))
                                        {
                                        }
                                    }
                                }
                                this.bindingManager.OnPostGenerateDelegate(bindingInfo);
                            }

                            // 提供委托与 Dispatcher 的桥接 (废弃)
                            // this.tsDeclare.AppendLine("declare namespace {0} {{", NamespaceOfInternalScriptTypes);
                            // this.tsDeclare.AddTabLevel();
                            // foreach (var spec in specs)
                            // {
                            //     var argtypelist = "";
                            //     var argdecllist = "";
                            //     var argvarlist = "";
                            //     for (var i = 0; i < spec.Key; i++)
                            //     {
                            //         argtypelist += $", T{i + 1}";
                            //         argdecllist += $"arg{i + 1}: T{i + 1}";
                            //         argvarlist += $"arg{i + 1}";
                            //         if (i != spec.Key - 1)
                            //         {
                            //             argdecllist += ", ";
                            //             argvarlist += ", ";
                            //         }
                            //     }
                            //     this.tsDeclare.AppendLine($"class Delegate{spec.Key}<R{argtypelist}> extends jsb.Dispatcher {{");
                            //     this.tsDeclare.AddTabLevel();
                            //     {
                            //         this.tsDeclare.AppendLine($"on(caller: any, fn: ({argdecllist}) => R): Delegate{spec.Key}<R{argtypelist}>");
                            //         this.tsDeclare.AppendLine($"off(caller: any, fn: ({argdecllist}) => R): void");
                            //         this.tsDeclare.AppendLine($"dispatch({argdecllist}): R");
                            //     }
                            //     this.tsDeclare.DecTabLevel();
                            //     this.tsDeclare.AppendLine("}");
                            // }
                            // this.tsDeclare.DecTabLevel();
                            // this.tsDeclare.AppendLine("}");
                        }
                    }
                }
            }
        }

        // 生成类型绑定
        public void Generate(TypeBindingInfo typeBindingInfo)
        {
            this.cs.enabled = (typeBindingInfo.bindingFlags & TypeBindingFlags.BindingCode) != 0 && (typeBindingFlags & TypeBindingFlags.BindingCode) != 0;
            this.tsDeclare.enabled = (typeBindingInfo.bindingFlags & TypeBindingFlags.TypeDefinition) != 0 && (typeBindingFlags & TypeBindingFlags.TypeDefinition) != 0;

            using (new EditorOnlyCodeGen(this, typeBindingInfo.isEditorRuntime))
            {
                GenerateInternal(typeBindingInfo);
            }
        }

        private void GenerateInternal(TypeBindingInfo typeBindingInfo)
        {
            using (new PlatformCodeGen(this, TypeBindingFlags.Default))
            {
                using (new TopLevelCodeGen(this, typeBindingInfo))
                {
                    using (new NamespaceCodeGen(this, this.bindingManager.prefs.ns, typeBindingInfo.jsNamespace))
                    {
                        if (typeBindingInfo.IsEnum)
                        {
                            using (new EnumCodeGen(this, typeBindingInfo))
                            {
                            }
                        }
                        else
                        {
                            using (new ClassCodeGen(this, typeBindingInfo))
                            {
                            }
                        }
                    }
                }
            }
        }

        private void WriteAllText(string path, string contents)
        {
            // if (File.Exists(path))
            // {
            //     var old = File.ReadAllText(path);
            //     if (old == contents)
            //     {
            //         return;
            //     }
            // }
            File.WriteAllText(path, contents);
            this.bindingManager.Info($"output file: {path} ({contents.Length})");
        }

        private void CopyFile(string srcPath, string dir, string filename)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.Copy(srcPath, Path.Combine(dir, filename));
        }

        public void WriteCSharp(string csOutDir, string filename, string tx)
        {
            try
            {
                if (this.cs.enabled && this.cs.size > 0)
                {
                    var csName = filename + ".cs" + tx;
                    var csPath = Path.Combine(csOutDir, csName);
                    this.bindingManager.AddOutputFile(csOutDir, csPath);

                    if (!Directory.Exists(csOutDir))
                    {
                        Directory.CreateDirectory(csOutDir);
                    }
                    WriteAllText(csPath, this.cs.ToString());
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write csharp file failed [{0}]: {1}", filename, exception.Message);
            }
        }

        public void WriteTSD(string tsOutDir, string tx)
        {
            try
            {
                if (bindingManager.prefs.singleTSD)
                {
                    if (this.tsDeclare.enabled && this.tsDeclare.size > 0)
                    {
                        var tsName = "jsb.autogen.d.ts" + tx;
                        var tsPath = Path.Combine(tsOutDir, tsName);
                        this.bindingManager.AddOutputFile(tsOutDir, tsPath);
                        
                        if (!Directory.Exists(tsOutDir))
                        {
                            Directory.CreateDirectory(tsOutDir);
                        }
                        WriteAllText(tsPath, this.tsDeclare.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write typescript declaration file failed [{0}]: {1}", "jsb.autogen", exception.Message);
            }
        }

        public void WriteTSD(string tsOutDir, string filename, string tx)
        {
            try
            {
                if (!bindingManager.prefs.singleTSD)
                {
                    if (this.tsDeclare.enabled && this.tsDeclare.size > 0)
                    {
                        var tsName = filename + ".d.ts" + tx;
                        var tsPath = Path.Combine(tsOutDir, tsName);
                        this.bindingManager.AddOutputFile(tsOutDir, tsPath);

                        if (!Directory.Exists(tsOutDir))
                        {
                            Directory.CreateDirectory(tsOutDir);
                        }
                        WriteAllText(tsPath, this.tsDeclare.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write typescript declaration file failed [{0}]: {1}", filename, exception.Message);
            }
        }

        // type: csharp 方法本身返回值的类型
        // value: csharp 方法本身的返回值名字
        public string AppendValuePusher(Type type, string value)
        {
            // if (type.IsEnum)
            // {
            //     var eType = type.GetEnumUnderlyingType();
            //     var eTypeName = this.bindingManager.GetCSTypeFullName(eType);
            //     return $"{this.bindingManager.GetScriptObjectPusher(eType)}(ctx, ({eTypeName}){value})";
            // }
            // return $"{this.bindingManager.GetScriptObjectPusher(type)}(ctx, {value})";
            return this.bindingManager.GetScriptObjectPusher(type, "ctx", value);
        }

        public string AppendMethodReturnValuePusher(MethodBase method, Type returnType, string value)
        {
            var transform = bindingManager.GetTypeTransform(method.DeclaringType);
            if (transform != null)
            {
                var mrp = transform.GetMethodReturnPusher(method);
                if (mrp != null)
                {
                    return $"{mrp}(ctx, {value})";
                }
            }
            // if (returnType.IsEnum)
            // {
            //     var eType = returnType.GetEnumUnderlyingType();
            //     var eTypeName = this.bindingManager.GetCSTypeFullName(eType);
            //     return $"{this.bindingManager.GetScriptObjectPusher(eType)}(ctx, ({eTypeName}){value})";
            // }
            // return $"{this.bindingManager.GetScriptObjectPusher(returnType)}(ctx, {value})";
            return this.bindingManager.GetScriptObjectPusher(returnType, "ctx", value);
        }

        public string AppendGetThisCS(FieldBindingInfo bindingInfo)
        {
            return AppendGetThisCS(bindingInfo.isStatic, bindingInfo.fieldInfo.DeclaringType);
        }

        public string AppendGetThisCS(DelegateBindingInfo bindingInfo)
        {
            return AppendGetThisCS(bindingInfo.isStatic, bindingInfo.declaringType);
        }

        public string AppendGetThisCS(EventBindingInfo bindingInfo)
        {
            var isStatic = bindingInfo.isStatic;
            var declaringType = bindingInfo.declaringType;
            var caller = "";
            if (isStatic)
            {
                caller = this.bindingManager.GetCSTypeFullName(declaringType, false);
            }
            else
            {
                caller = "self";
                this.cs.AppendLine($"{this.bindingManager.GetCSTypeFullName(declaringType)} {caller};");
                // this.cs.AppendLine($"DuktapeDLL.duk_push_this(ctx);");
                var getter = this.bindingManager.GetScriptObjectGetter(declaringType, "ctx", "this_obj", caller);
                this.cs.AppendLine("{0};", getter);
                // this.cs.AppendLine($"DuktapeDLL.duk_pop(ctx);"); 
            }
            return caller;
        }

        public string AppendGetThisCS(MethodBase method, bool asExtensionAnyway)
        {
            if (method.IsConstructor)
            {
                return null;
            }
            if (asExtensionAnyway)
            {
                var parameters = method.GetParameters();
                return AppendGetThisCS(false, parameters[0].ParameterType);
            }
            return AppendGetThisCS(method.IsStatic, method.DeclaringType);
        }

        public string AppendGetThisCS(bool isStatic, Type declaringType)
        {
            var caller = "";
            if (isStatic)
            {
                caller = this.bindingManager.GetCSTypeFullName(declaringType, false);
            }
            else
            {
                caller = "self";
                this.cs.AppendLine($"{this.bindingManager.GetCSTypeFullName(declaringType)} {caller};");
                // this.cs.AppendLine($"DuktapeDLL.duk_push_this(ctx);");
                var getter = this.bindingManager.GetScriptObjectGetter(declaringType, "ctx", "this_obj", caller);
                this.cs.AppendLine("if (!{0})", getter);
                using (this.cs.CodeBlockScope())
                {
                    this.cs.AppendLine("throw new ThisBoundException();");
                }
                // this.cs.AppendLine($"DuktapeDLL.duk_pop(ctx);");
            }
            return caller;
        }

        public string AppendGetArgCount(bool isVararg)
        {
            if (isVararg)
            {
                var varName = "argc";
                // cs.AppendLine("var {0} = DuktapeDLL.duk_get_top(ctx);", varName);
                return varName;
            }
            return null;
        }

        public void AppendJSDoc(Type type)
        {
            if (bindingManager.prefs.doc)
            {
                var doc = DocResolver.GetDocBody(type);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }
            }
            var jsdoc = type.GetCustomAttribute(typeof(JSDocAttribute), false) as JSDocAttribute;
            if (jsdoc != null)
            {
                AppendJSDoc(jsdoc.lines);
            }
        }

        public void AppendJSDoc(PropertyInfo propertyInfo)
        {
            if (bindingManager.prefs.doc)
            {
                var doc = DocResolver.GetDocBody(propertyInfo);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }
            }
            var jsdoc = propertyInfo.GetCustomAttribute(typeof(JSDocAttribute), false) as JSDocAttribute;
            if (jsdoc != null)
            {
                AppendJSDoc(jsdoc.lines);
            }
        }

        public void AppendJSDoc(FieldInfo fieldInfo)
        {
            if (bindingManager.prefs.doc)
            {
                var doc = DocResolver.GetDocBody(fieldInfo);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }
            }
            var jsdoc = fieldInfo.GetCustomAttribute(typeof(JSDocAttribute), false) as JSDocAttribute;
            if (jsdoc != null)
            {
                AppendJSDoc(jsdoc.lines);
            }
        }

        public void AppendEnumJSDoc(Type type, object value)
        {
            if (bindingManager.prefs.doc)
            {
                var resolver = DocResolver.GetResolver(type.Assembly);
                var doc = resolver.GetFieldDocBody(type.FullName + "." + Enum.GetName(type, value));
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }
            }
        }

        public void AppendJSDoc<T>(T methodInfo)
        where T : MethodBase
        {
            if (bindingManager.prefs.doc)
            {
                var doc = DocResolver.GetDocBody(methodInfo);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }
            }
            var jsdoc = methodInfo.GetCustomAttribute(typeof(JSDocAttribute), false) as JSDocAttribute;
            if (jsdoc != null)
            {
                AppendJSDoc(jsdoc.lines);
            }
        }

        public void AppendJSDoc(DocResolver.DocBody body)
        {
            if (body.summary.Length > 1)
            {
                this.tsDeclare.AppendLine("/**");
                foreach (var line in body.summary)
                {
                    this.tsDeclare.AppendLine(" * {0}", line.Replace('\r', ' '));
                }
            }
            else
            {
                if (body.summary.Length == 0 || string.IsNullOrEmpty(body.summary[0]))
                {
                    if (body.parameters.Count == 0 && string.IsNullOrEmpty(body.returns))
                    {
                        return;
                    }
                    this.tsDeclare.AppendLine("/**");
                }
                else
                {
                    this.tsDeclare.AppendLine("/** {0}", body.summary[0]);
                }
            }
            foreach (var kv in body.parameters)
            {
                var pname = kv.Key;
                var ptext = kv.Value;
                this.tsDeclare.AppendLine($" * @param {pname} {ptext}");
            }
            if (!string.IsNullOrEmpty(body.returns))
            {
                this.tsDeclare.AppendLine($" * @returns {body.returns}");
            }
            this.tsDeclare.AppendLine(" */");
        }

        public void AppendJSDoc(string[] lines)
        {
            if (lines != null && lines.Length > 0)
            {
                if (lines.Length > 1)
                {
                    this.tsDeclare.AppendLine("/**");
                    foreach (var line in lines)
                    {
                        this.tsDeclare.AppendLine(" * {0}", line.Replace('\r', ' '));
                    }
                }
                else
                {
                    this.tsDeclare.AppendLine("/** {0}", lines[0]);
                }
                this.tsDeclare.AppendLine(" */");
            }
        }
    }
}