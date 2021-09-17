using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace QuickJS.Binding
{
    using QuickJS.Binding;

    public partial class CodeGenerator
    {
        public const string NameOfDelegates = "_QuickJSDelegates";
        public const string NameOfHotfixDelegates = "_QuickJSHotfixDelegates";
        public const string NameOfBindingList = "_QuickJSBindings";
        public const string NamespaceOfScriptTypes = "QuickJS";
        public const string NamespaceOfInternalScriptTypes = "QuickJS.Internal";
        public const string NameOfBuffer = "ArrayBuffer";

        private TSModuleCodeGen _currentTSModule = null;

        public TSModuleCodeGen currentTSModule => _currentTSModule;
        public BindingManager bindingManager;
        public TextGenerator cs;
        public TextGenerator tsDeclare;
        public JSBindResult bindResult;
        public TypeBindingFlags typeBindingFlags;

        public bool withCodegen => cs.enabled || tsDeclare.enabled;

        public CodeGenerator(BindingManager bindingManager, TypeBindingFlags typeBindingFlags)
        {
            this.typeBindingFlags = typeBindingFlags;
            this.bindingManager = bindingManager;
            var tab = this.bindingManager.prefs.tab;
            var newline = this.bindingManager.prefs.newline;

            bindResult = new JSBindResult();
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

        public void GenerateBindingList(IEnumerable<IGrouping<string, TypeBindingInfo>> modules)
        {
            this.cs.enabled = (typeBindingFlags & TypeBindingFlags.BindingCode) != 0;
            this.tsDeclare.enabled = (typeBindingFlags & TypeBindingFlags.TypeDefinition) != 0;

            if (!withCodegen)
            {
                return;
            }

            using (new CSDebugCodeGen(this))
            {
                using (new CSPlatformCodeGen(this, TypeBindingFlags.Default))
                {
                    using (new CSTopLevelCodeGen(this, CodeGenerator.NameOfBindingList))
                    {
                        using (new CSNamespaceCodeGen(this, typeof(Values).Namespace))
                        {
                            using (new PlainClassCodeGen(this, typeof(Values).Name))
                            {
                                using (new PreservedCodeGen(this))
                                {
                                    this.cs.AppendLine("public const uint CodeGenVersion = {0};", ScriptEngine.VERSION);
                                }

                                using (new PreservedCodeGen(this))
                                {
                                    using (var method = new PlainMethodCodeGen(this, "private static void BindAll(ScriptRuntime runtime)"))
                                    {
                                        foreach (var module in modules)
                                        {
                                            if (module.Count() > 0)
                                            {
                                                var moduleName = string.IsNullOrEmpty(module.Key) ? this.bindingManager.prefs.defaultJSModule : module.Key;
                                                var runtimeVarName = "rt";
                                                var moduleVarName = "module";
                                                this.cs.AppendLine($"runtime.AddStaticModuleProxy(\"{moduleName}\", ({runtimeVarName}, {moduleVarName}) => ");
                                                this.bindResult.modules.Add(moduleName);

                                                using (this.cs.TailCallCodeBlockScope())
                                                {
                                                    var editorTypesMap = new Dictionary<string, List<TypeBindingInfo>>();
                                                    foreach (var type in module)
                                                    {
                                                        var requiredDefinesOfType = type.transform.requiredDefines;
                                                        if (requiredDefinesOfType != null)
                                                        {
                                                            var defs = string.Join(" && ", from def in requiredDefinesOfType select def);
                                                            List<TypeBindingInfo> list;
                                                            if (!editorTypesMap.TryGetValue(defs, out list))
                                                            {
                                                                editorTypesMap[defs] = list = new List<TypeBindingInfo>();
                                                            }
                                                            list.Add(type);
                                                        }
                                                        else
                                                        {
                                                            method.AddModuleEntry(moduleName, runtimeVarName, moduleVarName, type);
                                                        }
                                                    }

                                                    foreach (var editorTypes in editorTypesMap)
                                                    {
                                                        using (new CSEditorOnlyCodeGen(this, editorTypes.Key))
                                                        {
                                                            foreach (var type in editorTypes.Value)
                                                            {
                                                                method.AddModuleEntry(moduleName, runtimeVarName, moduleVarName, type);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        method.AddStatement("{0}.{1}.Bind(runtime);", this.bindingManager.prefs.ns, CodeGenerator.NameOfDelegates);
                                    } // func: BindAll
                                } // 'preserved' attribute for func: BindAll
                            } // class 
                        } // cs-namespace
                    } // toplevel
                } // platform
            } // debug

            this.bindResult.comment = "this file was generated by CodeGenerator";
            if (!string.IsNullOrEmpty(this.bindingManager.prefs.jsModulePackInfoPath))
            {
                WriteJSON(this.bindingManager.prefs.jsModulePackInfoPath, this.bindResult);
            }
        }

        // 生成委托绑定
        public void Generate(DelegateBridgeBindingInfo[] delegateBindingInfos, List<HotfixDelegateBindingInfo> exportedHotfixDelegates)
        {
            this.cs.enabled = (typeBindingFlags & TypeBindingFlags.BindingCode) != 0;
            this.tsDeclare.enabled = (typeBindingFlags & TypeBindingFlags.TypeDefinition) != 0;

            if (!withCodegen)
            {
                return;
            }

            using (new CSDebugCodeGen(this))
            {
                using (new CSPlatformCodeGen(this, TypeBindingFlags.Default))
                {
                    using (new CSTopLevelCodeGen(this, CodeGenerator.NameOfDelegates))
                    {
                        using (new CSNamespaceCodeGen(this, this.bindingManager.prefs.ns))
                        {
                            using (new DelegateWrapperCodeGen(this))
                            {
                                for (var i = 0; i < exportedHotfixDelegates.Count; i++)
                                {
                                    var hotfixDelegateBindingInfo = exportedHotfixDelegates[i];

                                    using (new PreservedCodeGen(this))
                                    {
                                        using (new HotfixDelegateCodeGen(this, hotfixDelegateBindingInfo, i))
                                        {
                                        }
                                    }
                                }

                                for (var i = 0; i < delegateBindingInfos.Length; i++)
                                {
                                    var delegateBindingInfo = delegateBindingInfos[i];
                                    // var nargs = delegateBindingInfo.parameters.Length;

                                    this.bindingManager.OnPreGenerateDelegate(delegateBindingInfo);
                                    using (new CSEditorOnlyCodeGen(this, delegateBindingInfo.requiredDefines))
                                    {
                                        using (new PreservedCodeGen(this))
                                        {
                                            using (new DelegateCodeGen(this, delegateBindingInfo, i))
                                            {
                                            }
                                        }
                                    }
                                    this.bindingManager.OnPostGenerateDelegate(delegateBindingInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Begin()
        {
        }

        public void End()
        {
        }

        // 生成类型绑定
        public void Generate(TypeBindingInfo typeBindingInfo)
        {
            this.cs.enabled = (typeBindingInfo.bindingFlags & TypeBindingFlags.BindingCode) != 0 && (typeBindingFlags & TypeBindingFlags.BindingCode) != 0;
            this.tsDeclare.enabled = (typeBindingInfo.bindingFlags & TypeBindingFlags.TypeDefinition) != 0 && (typeBindingFlags & TypeBindingFlags.TypeDefinition) != 0;
            
            if (!withCodegen)
            {
                return;
            }

            using (new CSDebugCodeGen(this))
            {
                using (new CSEditorOnlyCodeGen(this, typeBindingInfo.transform.requiredDefines))
                {
                    using (new CSPlatformCodeGen(this, TypeBindingFlags.Default))
                    {
                        using (new CSTopLevelCodeGen(this, typeBindingInfo))
                        {
                            using (new CSNamespaceCodeGen(this, this.bindingManager.prefs.ns))
                            {
                                GenerateInternal(typeBindingInfo);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateInternal(TypeBindingInfo typeBindingInfo)
        {
            using (var tsMod = new TSModuleCodeGen(this, typeBindingInfo))
            {
                _currentTSModule = tsMod;
                using (new TSNamespaceCodeGen(this, typeBindingInfo.tsTypeNaming.jsNamespace))
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
                _currentTSModule = null;
            }
        }

        private void WriteAllText(string path, TextGenerator gen)
        {
            // if (File.Exists(path))
            // {
            //     var old = File.ReadAllText(path);
            //     if (old == contents)
            //     {
            //         return;
            //     }
            // }
            var contents = gen.Submit();
            File.WriteAllText(path, contents);
            this.bindingManager.Info($"output file: {path} ({contents.Length})");
        }

        private void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
            this.bindingManager.Info($"output file: {path} ({contents.Length})");
        }

        private void WriteJSON<T>(string path, T obj)
        {
            try
            {
                var contents = bindingManager.json.Serialize(obj, true);
                File.WriteAllText(path, contents);
                this.bindingManager.Info($"output file: {path} ({contents.Length})");
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write json failed [{0}]: {1}\n{2}", typeof(T), path, exception.Message);
            }
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
                    WriteAllText(csPath, this.cs);
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write csharp file failed [{0}]: {1}", filename, exception.Message);
            }
        }

        public void WriteTSD(string tsOutDir, string tx)
        {
            var mod = "jsb.autogen";
            try
            {
                if (bindingManager.prefs.singleTSD)
                {
                    if (this.tsDeclare.enabled && this.tsDeclare.size > 0)
                    {
                        var tsName = mod + ".d.ts" + tx;
                        var tsPath = Path.Combine(tsOutDir, tsName);
                        this.bindingManager.AddOutputFile(tsOutDir, tsPath);

                        if (!Directory.Exists(tsOutDir))
                        {
                            Directory.CreateDirectory(tsOutDir);
                        }
                        WriteAllText(tsPath, this.tsDeclare);
                    }
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write typescript declaration file failed [{0}]: {1}", mod, exception.Message);
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
                        WriteAllText(tsPath, this.tsDeclare);
                    }
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write typescript declaration file failed [{0}]: {1}", filename, exception.Message);
            }
        }

        // 对参数进行取值, 如果此参数无需取值, 则返回 false
        // methodBase: (optional) 提供辅助异常信息
        public bool WriteParameterGetter(ParameterInfo parameter, int index, string argname, MethodBase methodBase)
        {
            // 非 out 参数才需要取值
            var argAssign = !parameter.IsOut || !parameter.ParameterType.IsByRef;
            return WriteParameterGetter(parameter, index, true, argname, argAssign, methodBase);
        }

        public bool WriteParameterGetter(ParameterInfo parameter, int index, bool argDecl, string argname, bool argAssign, MethodBase methodBase)
        {
            var ptype = parameter.ParameterType;
            var argTypeStr = this.bindingManager.GetCSTypeFullName(parameter.ParameterType);

            if (argDecl)
            {
                this.cs.AppendLine($"{argTypeStr} {argname};");
            }

            if (argAssign)
            {
                if (ptype == typeof(Native.JSContext))
                {
                    this.cs.AppendLine("{0} = ctx;", argname);
                    return false;
                }

                if (ptype == typeof(ScriptContext))
                {
                    this.cs.AppendLine("{0} = {1}.GetContext(ctx);", argname, nameof(ScriptEngine));
                    return false;
                }

                if (ptype == typeof(Native.JSRuntime))
                {
                    this.cs.AppendLine("{0} = JSApi.JS_GetRuntime(ctx);", argname);
                    return false;
                }

                if (ptype == typeof(ScriptRuntime))
                {
                    this.cs.AppendLine("{0} = {1}.GetRuntime(ctx);", argname, nameof(ScriptEngine));
                    return false;
                }

                var isRefWrapper = parameter.ParameterType.IsByRef && !parameter.IsOut;

                // process ref parameter get
                string getVal;
                string refValVar = null;
                if (isRefWrapper)
                {
                    refValVar = $"refVal{index}";
                    this.cs.AppendLine("var {0} = js_read_wrap(ctx, argv[{1}]);", refValVar, index);
                    getVal = refValVar;

                    this.cs.AppendLine("if ({0}.IsException())", refValVar);
                    using (this.cs.CodeBlockScope())
                    {
                        this.cs.AppendLine("return {0};", refValVar);
                    }
                }
                else
                {
                    getVal = $"argv[{index}]";
                }
                var getter = this.bindingManager.GetScriptObjectGetter(ptype, "ctx", getVal, argname);
                this.cs.AppendLine("if (!{0})", getter);
                using (this.cs.CodeBlockScope())
                {
                    if (isRefWrapper)
                    {
                        this.cs.AppendLine("JSApi.JS_FreeValue(ctx, {0});", refValVar);
                    }
                    this.WriteParameterException(methodBase, argTypeStr, index);
                }
                if (isRefWrapper)
                {
                    this.cs.AppendLine("JSApi.JS_FreeValue(ctx, {0});", refValVar);
                }
                return true;
            }

            return true;
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
            // var transform = bindingManager.GetTypeTransform(method.DeclaringType);
            // if (transform != null)
            // {
            //     var mrp = transform.GetMethodReturnPusher(method);
            //     if (mrp != null)
            //     {
            //         return $"{mrp}(ctx, {value})";
            //     }
            // }
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
                var getter = this.bindingManager.GetScriptObjectGetter(declaringType, "ctx", "this_obj", caller);
                this.cs.AppendLine("{0};", getter);
            }
            return caller;
        }

        public string AppendGetThisCS(MethodBase method, bool asExtensionAnyway, out Type thisType)
        {
            if (method.IsConstructor)
            {
                thisType = null;
                return null;
            }

            if (asExtensionAnyway)
            {
                var parameters = method.GetParameters();
                thisType = parameters[0].ParameterType;
                return AppendGetThisCS(false, parameters[0].ParameterType);
            }

            thisType = method.DeclaringType;
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
                if (this.cs.enabled)
                {
                    this.cs.AppendLine($"{this.bindingManager.GetCSTypeFullName(declaringType)} {caller};");
                    var getter = this.bindingManager.GetScriptObjectGetter(declaringType, "ctx", "this_obj", caller);
                    this.cs.AppendLine("if (!{0})", getter);
                    using (this.cs.CodeBlockScope())
                    {
                        this.cs.AppendLine("throw new ThisBoundException();");
                    }
                }
            }
            return caller;
        }

        public string AppendGetArgCount(bool isVararg)
        {
            if (isVararg)
            {
                var varName = "argc";
                return varName;
            }
            return null;
        }

        public void AppendJSDoc(Type type)
        {
            if (bindingManager.prefs.genTypescriptDoc && this.tsDeclare.enabled)
            {
                var doc = this.GetDocBody(type);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }

                var jsdoc = Attribute.GetCustomAttribute(type, typeof(JSDocAttribute), false) as JSDocAttribute;
                if (jsdoc != null)
                {
                    AppendJSDoc(jsdoc.lines);
                }
            }
        }

        public void AppendJSDoc(PropertyInfo propertyInfo)
        {
            if (bindingManager.prefs.genTypescriptDoc && this.tsDeclare.enabled)
            {
                var doc = this.GetDocBody(propertyInfo);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }

                var jsdoc = Attribute.GetCustomAttribute(propertyInfo, typeof(JSDocAttribute), false) as JSDocAttribute;
                if (jsdoc != null)
                {
                    AppendJSDoc(jsdoc.lines);
                }
            }
        }

        public void AppendJSDoc(FieldInfo fieldInfo)
        {
            if (bindingManager.prefs.genTypescriptDoc && this.tsDeclare.enabled)
            {
                var doc = this.GetDocBody(fieldInfo);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }

                var jsdoc = Attribute.GetCustomAttribute(fieldInfo, typeof(JSDocAttribute), false) as JSDocAttribute;
                if (jsdoc != null)
                {
                    AppendJSDoc(jsdoc.lines);
                }
            }
        }

        public void AppendEnumJSDoc(Type type, object value)
        {
            if (bindingManager.prefs.genTypescriptDoc && this.tsDeclare.enabled)
            {
                var resolver = this.GetResolver(type.Assembly);
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
            if (bindingManager.prefs.genTypescriptDoc && this.tsDeclare.enabled)
            {
                var doc = this.GetDocBody(methodInfo);
                if (doc != null)
                {
                    AppendJSDoc(doc);
                    return;
                }

                var jsdoc = Attribute.GetCustomAttribute(methodInfo, typeof(JSDocAttribute), false) as JSDocAttribute;
                if (jsdoc != null)
                {
                    AppendJSDoc(jsdoc.lines);
                }
            }
        }

        public void AppendJSDoc(DocResolver.DocBody body)
        {
            if (body.summary != null && body.summary.Length > 1)
            {
                this.tsDeclare.AppendLine("/**");
                foreach (var line in body.summary)
                {
                    this.tsDeclare.AppendLine(" * {0}", line.Replace('\r', ' '));
                }
            }
            else
            {
                if (body.summary == null || body.summary.Length == 0 || string.IsNullOrEmpty(body.summary[0]))
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

            if (body.parameters != null)
            {
                foreach (var kv in body.parameters)
                {
                    var pname = kv.Key;
                    var ptext = kv.Value;
                    this.tsDeclare.AppendLine($" * @param {pname} {ptext}");
                }
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

        public void WriteParameterException(MethodBase methodBase, Type argType, int argIndex)
        {
            WriteParameterException(methodBase.DeclaringType, methodBase.Name, argType, argIndex);
        }

        public void WriteParameterException(MethodBase methodBase, string argTypeStr, int argIndex)
        {
            if (methodBase != null)
            {
                WriteParameterException(methodBase.DeclaringType, methodBase.Name, argTypeStr, argIndex);
            }
            else
            {
                WriteParameterException("?", "?", argTypeStr, argIndex);
            }
        }

        public void WriteParameterException(Type argType, int argIndex)
        {
            var argTypeStr = this.bindingManager.GetCSTypeFullName(argType);
            WriteParameterException(argTypeStr, argIndex);
        }

        public void WriteParameterException(string argTypeStr, int argIndex)
        {
            this.cs.AppendLine("throw new ParameterException(typeof({0}), {1});", argTypeStr, argIndex);
        }

        public void WriteParameterException(Type caller, string method, Type argType, int argIndex)
        {
            var callerStr = this.bindingManager.GetCSTypeFullName(caller);
            var argTypeStr = this.bindingManager.GetCSTypeFullName(argType);
            WriteParameterException(callerStr, method, argTypeStr, argIndex);
        }

        public void WriteParameterException(Type caller, string method, string argTypeStr, int argIndex)
        {
            var callerStr = this.bindingManager.GetCSTypeFullName(caller);
            WriteParameterException(callerStr, method, argTypeStr, argIndex);
        }

        public void WriteParameterException(string callerStr, string method, string argTypeStr, int argIndex)
        {
            this.cs.AppendLine("throw new ParameterException(typeof({0}), \"{1}\", typeof({2}), {3});", callerStr, method, argTypeStr, argIndex);
        }
    }
}