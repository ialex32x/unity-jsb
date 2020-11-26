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

    public abstract class MethodBaseCodeGen<T> : IDisposable
    where T : MethodBase
    {
        protected CodeGenerator cg;

        // 方法参数数比较, 用于列表排序
        public static int MethodComparer(T a, T b)
        {
            var va = a.GetParameters().Length;
            var vb = b.GetParameters().Length;
            return va > vb ? 1 : ((va == vb) ? 0 : -1);
        }

        public MethodBaseCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
        }

        protected virtual void OnBegin()
        {
        }

        public virtual void Dispose()
        {
        }

        public string GetParamArrayMatchType(T method)
        {
            var parameters = method.GetParameters();
            var parameter = parameters[parameters.Length - 1];
            var typename = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType.GetElementType());
            return $"typeof({typename})";
        }

        // 生成定参部分 type 列表 (首参前也会补",")
        public string GetFixedMatchTypes(T method, bool isVararg, bool isExtension)
        {
            var snippet = "";
            var parameters = method.GetParameters();
            var pIndex = isExtension ? 1 : 0;
            var length = isVararg ? parameters.Length - 1 : parameters.Length;
            var argIndex = 0;

            for (; pIndex < length; pIndex++)
            {
                var parameter = parameters[pIndex];
                var typename = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType);

                if (parameter.ParameterType.IsByRef)
                {
                    snippet += $"js_match_type_hint(ctx, argv[{argIndex}], typeof({typename}))";
                }
                else
                {
                    snippet += $"js_match_type(ctx, argv[{argIndex}], typeof({typename}))";
                }

                if (pIndex != length - 1)
                {
                    snippet += " && ";
                }

                argIndex++;
            }

            return snippet;
        }

        public string Concat(List<string> args, string sp = ", ")
        {
            var len = args.Count;
            var res = "";
            for (var i = 0; i < len; i++)
            {
                res += args[i];
                if (i != len - 1)
                {
                    res += sp;
                }
            }

            return res;
        }

        // hasParams: 是否包含变参 (最后一个参数将按数组处理)
        public List<string> AppendGetParameters(bool hasParams, string nargs, ParameterInfo[] parameters)
        {
            var arglist = new List<string>();
            var assignIndex = 0;
            for (var i = 0; i < parameters.Length; i++)
            {
                var argitem = "";
                var parameter = parameters[i];
                if (parameter.IsOut && parameter.ParameterType.IsByRef)
                {
                    argitem += "out ";
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    argitem += "ref ";
                }
                argitem += "arg" + i;
                arglist.Add(argitem);
                if (hasParams && i == parameters.Length - 1)
                {
                    // 处理数组
                    var argType = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType);
                    var argElementType = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType.GetElementType());
                    var argElementIndex = i == 0 ? nargs : nargs + " - " + i;
                    this.cg.cs.AppendLine($"{argType} arg{i} = null;");
                    this.cg.cs.AppendLine($"if ({argElementIndex} > 0)");
                    using (this.cg.cs.CodeBlockScope())
                    {
                        this.cg.cs.AppendLine($"arg{i} = new {argElementType}[{argElementIndex}];");
                        this.cg.cs.AppendLine($"for (var i = {i}; i < {nargs}; i++)");
                        using (this.cg.cs.CodeBlockScope())
                        {
                            var argElementOffset = i == 0 ? "" : " - " + i;
                            var argName = $"arg{i}[i{argElementOffset}]";
                            var argGetter = this.cg.bindingManager.GetScriptObjectGetter(parameter.ParameterType.GetElementType(), "ctx", "argv[i]", argName);
                            this.cg.cs.AppendLine("{0};", argGetter);
                        }
                    }
                }
                else
                {
                    if (WriteParameterGetter(parameter, assignIndex, $"arg{i}"))
                    {
                        assignIndex++;
                    }
                }
            }
            return arglist;
        }

        protected virtual void InvokeVoidReturn()
        {
            cg.cs.AppendLine("return JSApi.JS_UNDEFINED;");
        }

        // 对参数进行取值, 如果此参数无需取值, 则返回 false
        protected bool WriteParameterGetter(ParameterInfo parameter, int index, string argname)
        {
            var ptype = parameter.ParameterType;
            var argType = this.cg.bindingManager.GetCSTypeFullName(ptype);

            this.cg.cs.AppendLine($"{argType} {argname};");
            // 非 out 参数才需要取值
            if (!parameter.IsOut || !parameter.ParameterType.IsByRef)
            {
                if (ptype == typeof(Native.JSContext))
                {
                    this.cg.cs.AppendLine("{0} = ctx;", argname);
                    return false;
                }

                if (ptype == typeof(ScriptContext))
                {
                    this.cg.cs.AppendLine("{0} = {1}.GetContext(ctx);", argname, nameof(ScriptEngine));
                    return false;
                }

                if (ptype == typeof(Native.JSRuntime))
                {
                    this.cg.cs.AppendLine("{0} = JSApi.JS_GetRuntime(ctx);", argname);
                    return false;
                }

                if (ptype == typeof(ScriptRuntime))
                {
                    this.cg.cs.AppendLine("{0} = {1}.GetRuntime(ctx);", argname, nameof(ScriptEngine));
                    return false;
                }

                var isRefWrapper = parameter.ParameterType.IsByRef && !parameter.IsOut;

                // process ref parameter get
                string getVal;
                string refValVar = null;
                if (isRefWrapper)
                {
                    refValVar = $"refVal{index}";
                    this.cg.cs.AppendLine("var {0} = js_read_wrap(ctx, argv[{1}]);", refValVar, index);
                    getVal = refValVar;

                    this.cg.cs.AppendLine("if ({0}.IsException())", refValVar);
                    using (this.cg.cs.CodeBlockScope())
                    {
                        this.cg.cs.AppendLine("return {0};", refValVar);
                    }
                }
                else
                {
                    getVal = $"argv[{index}]";
                }
                var getter = this.cg.bindingManager.GetScriptObjectGetter(ptype, "ctx", getVal, argname);
                this.cg.cs.AppendLine("if (!{0})", getter);
                using (this.cg.cs.CodeBlockScope())
                {
                    if (isRefWrapper)
                    {
                        this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, {0});", refValVar);
                    }
                    this.cg.cs.AppendLine("throw new ParameterException(typeof({0}), {1});", argType, index);
                }
                if (isRefWrapper)
                {
                    this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, {0});", refValVar);
                }
                return true;
            }

            return true;
        }

        // 输出所有变体绑定
        // hasOverrides: 是否需要处理重载
        protected void WriteCSAllVariants(MethodBaseBindingInfo<T> methodBindingInfo) // SortedDictionary<int, MethodBaseVariant<T>> variants)
        {
            var variants = methodBindingInfo.variants;
            var prefs = cg.bindingManager.prefs;

            if (prefs.alwaysCheckArgc || /*hasOverrides*/ methodBindingInfo.count > 1)
            {
                // 需要处理重载
                GenMethodVariants(methodBindingInfo, variants);
            }
            else
            {
                // 没有重载的情况 (variants.Count == 1)
                foreach (var variantKV in variants)
                {
                    var args = variantKV.Key;
                    var variant = variantKV.Value;
                    var argc = cg.AppendGetArgCount(variant.isVararg);

                    if (variant.isVararg)
                    {
                        var method = variant.varargMethods[0];
                        WriteCSMethodBinding(methodBindingInfo, method.method, argc, true, method.isExtension);
                    }
                    else
                    {
                        var method = variant.plainMethods[0];
                        WriteCSMethodBinding(methodBindingInfo, method.method, argc, false, method.isExtension);
                    }
                }
            }
        }

        protected void WriteTSAllVariants(TypeBindingInfo typeBindingInfo, MethodBaseBindingInfo<T> bindingInfo)
        {
            var variants = bindingInfo.variants;
            //NOTE: 如果产生了无法在 typescript 中声明的方法, 则作标记, 并输出一条万能声明 
            //      [key: string]: any
            foreach (var variantKV in variants)
            {
                foreach (var method in variantKV.Value.plainMethods)
                {
                    WriteTSDeclaration(typeBindingInfo, method.method, bindingInfo, method.isExtension);
                }

                foreach (var method in variantKV.Value.varargMethods)
                {
                    WriteTSDeclaration(typeBindingInfo, method.method, bindingInfo, method.isExtension);
                }
            }
        }

        // 写入返回类型声明
        protected virtual void WriteTSReturn(T method, List<ParameterInfo> returnParameters)
        {
            var returnType = GetReturnType(method);
            var outParametersCount = returnParameters.Count;
            if (returnType != null && returnType != typeof(void))
            {
                if (outParametersCount != 0)
                {
                    this.cg.tsDeclare.AppendL(": { ");

                    var returnTypeTS = this.cg.currentTSModule.GetTSTypeFullName(returnType);
                    var returnVarName = this.cg.bindingManager.GetTSVariable("return");
                    this.cg.tsDeclare.AppendL($"\"{returnVarName}\": {returnTypeTS}");

                    for (var i = 0; i < outParametersCount; i++)
                    {
                        var rp = returnParameters[i];
                        var name = this.cg.bindingManager.GetTSVariable(rp.Name);
                        var ts = this.cg.currentTSModule.GetTSTypeFullName(rp.ParameterType);
                        if (i != outParametersCount - 1)
                        {
                            this.cg.tsDeclare.AppendL($", \"{name}\": {ts}");
                        }
                        else
                        {
                            this.cg.tsDeclare.AppendL($", \"{name}\": {ts}");
                        }
                    }
                    this.cg.tsDeclare.AppendL(" }");
                    this.cg.tsDeclare.AppendLine();
                }
                else
                {
                    var returnTypeTS = this.cg.currentTSModule.GetTSTypeFullName(returnType);
                    this.cg.tsDeclare.AppendL($": {returnTypeTS}");
                    this.cg.tsDeclare.AppendLine();
                }
            }
            else
            {
                if (outParametersCount != 0)
                {
                    this.cg.tsDeclare.AppendL(": { ");
                    for (var i = 0; i < outParametersCount; i++)
                    {
                        var rp = returnParameters[i];
                        var name = rp.Name;
                        var ts = this.cg.currentTSModule.GetTSTypeFullName(rp.ParameterType);
                        if (i != outParametersCount - 1)
                        {
                            this.cg.tsDeclare.AppendL($"\"{name}\": {ts}, ");
                        }
                        else
                        {
                            this.cg.tsDeclare.AppendL($"\"{name}\": {ts}");
                        }
                    }
                    this.cg.tsDeclare.AppendL(" }");
                    this.cg.tsDeclare.AppendLine();
                }
                else
                {
                    if (method.IsConstructor)
                    {
                        this.cg.tsDeclare.AppendLine();
                    }
                    else
                    {
                        this.cg.tsDeclare.AppendL(": void");
                        this.cg.tsDeclare.AppendLine();
                    }
                }
            }
        }

        protected void GenMethodVariants(MethodBaseBindingInfo<T> methodBindingInfo, SortedDictionary<int, MethodBaseVariant<T>> variants)
        {
            var argc = cg.AppendGetArgCount(true);
            using (cg.cs.DoWhileBlockScope(methodBindingInfo.count > 1))
            {
                foreach (var variantKV in variants)
                {
                    var expectedArgCount = variantKV.Key;
                    var variant = variantKV.Value;
                    var gecheck = expectedArgCount > 0 && variant.isVararg; // 最后一组分支且存在变参时才需要判断 >= 

                    if (gecheck)
                    {
                        cg.cs.AppendLine("if (argc >= {0})", expectedArgCount);
                        cg.cs.AppendLine("{");
                        cg.cs.AddTabLevel();
                    }

                    // 处理定参
                    if (variant.plainMethods.Count > 0)
                    {
                        cg.cs.AppendLine("if (argc == {0})", expectedArgCount);
                        using (cg.cs.CodeBlockScope())
                        {
                            var prefs = cg.bindingManager.prefs;
                            if (prefs.alwaysCheckArgType || (variant.plainMethods.Count + variant.varargMethods.Count) > 1)
                            {
                                foreach (var method in variant.plainMethods)
                                {
                                    var fixedMatchers = GetFixedMatchTypes(method.method, false, method.isExtension);
                                    if (fixedMatchers.Length != 0)
                                    {
                                        cg.cs.AppendLine($"if ({fixedMatchers})");
                                        using (cg.cs.CodeBlockScope())
                                        {
                                            this.WriteCSMethodBinding(methodBindingInfo, method.method, argc, false, method.isExtension);
                                        }
                                    }
                                    else
                                    {
                                        this.WriteCSMethodBinding(methodBindingInfo, method.method, argc, false, method.isExtension);
                                    }
                                }

                                // if (methodBindingInfo.count > 1 && expectedArgCount != 0)
                                // {
                                //     cg.cs.AppendLine("break;");
                                // }
                            }
                            else
                            {
                                // 只有一个定参方法时, 不再判定类型匹配
                                var method = variant.plainMethods[0];
                                this.WriteCSMethodBinding(methodBindingInfo, method.method, argc, false, method.isExtension);
                            }
                        }
                    }

                    // 处理变参
                    if (variant.varargMethods.Count > 0)
                    {
                        foreach (var method in variant.varargMethods)
                        {
                            var fixedMatchers = GetFixedMatchTypes(method.method, true, method.isExtension);
                            var variantMatchers = GetParamArrayMatchType(method.method);

                            if (fixedMatchers.Length > 0)
                            {
                                cg.cs.AppendLine($"if ({fixedMatchers} && js_match_param_types(ctx, {expectedArgCount}, argv, {variantMatchers}))");
                            }
                            else
                            {
                                cg.cs.AppendLine($"if (js_match_param_types(ctx, {expectedArgCount}, argv, {variantMatchers}))");
                            }

                            using (cg.cs.CodeBlockScope())
                            {
                                this.WriteCSMethodBinding(methodBindingInfo, method.method, argc, true, method.isExtension);
                            }
                        }
                    }

                    if (gecheck)
                    {
                        cg.cs.DecTabLevel();
                        cg.cs.AppendLine("}");
                    }
                }
            }

            // error = return $"JSApi.JS_ThrowInternalError(ctx, \"{err}\")";
            cg.cs.AppendLine($"throw new NoSuitableMethodException(\"{methodBindingInfo.jsName}\", {argc});");
        }

        protected List<ParameterInfo> WriteTSDeclaration(TypeBindingInfo typeBindingInfo, T method, MethodBaseBindingInfo<T> bindingInfo, bool isExtension)
        {
            var refParameters = new List<ParameterInfo>();
            string tsMethodDeclaration;
            this.cg.AppendJSDoc(method);

            if (typeBindingInfo.transform.GetTSMethodDeclaration(method, out tsMethodDeclaration)
             || this.cg.bindingManager.GetTSMethodDeclaration(method, out tsMethodDeclaration))
            {
                this.cg.tsDeclare.AppendLine(tsMethodDeclaration);
                return refParameters;
            }

            string tsMethodRename;
            if (!this.cg.bindingManager.GetTSMethodRename(method, out tsMethodRename))
            {
                tsMethodRename = bindingInfo.jsName;
            }

            var isRaw = method.IsDefined(typeof(JSCFunctionAttribute));
            //TODO: 需要处理参数类型归并问题, 因为如果类型没有导入 ts 中, 可能会在声明中出现相同参数列表的定义
            //      在 MethodVariant 中创建每个方法对应的TS类型名参数列表, 完全相同的不再输出
            var prefix = "";
            // if (method.Name.StartsWith("op_"))
            if (bindingInfo is OperatorBindingInfo)
            {
                prefix += "// js_op_overloading: ";
            }
            else
            {
                // var baseType = typeBindingInfo.type.BaseType;
                // if (baseType != null)
                // {
                //     //TODO: 需要检查 TypeBindingInfo 对此的命名修改
                //     if (baseType.GetMethods().Where(baseMethodInfo => baseMethodInfo.Name == tsMethodRename).Count() != 0)
                //     {
                //         prefix += "// @ts-ignore" + this.cg.tsDeclare.newline + this.cg.tsDeclare.tabString;
                //     }
                // }
            }

            if (method.IsStatic && !isExtension)
            {
                prefix += "static ";
            }

            this.cg.tsDeclare.Append($"{prefix}{tsMethodRename}(");

            if (this.cg.bindingManager.prefs.verboseLog)
            {
                this.cg.bindingManager.Info($"WriteTSDeclaration: {method.Name} <isExtension: {isExtension}> => {tsMethodRename} {this.cg.tsDeclare.enabled}");
            }

            if (isRaw)
            {
                this.cg.tsDeclare.AppendL("...uncertain: any[]): any /* uncertain */");
                this.cg.tsDeclare.AppendLine();
            }
            else
            {
                var parameters = method.GetParameters();
                if (isExtension)
                {
                    ArrayUtility.RemoveAt(ref parameters, 0);
                }

                for (int i = 0, len = parameters.Length; i < len;)
                {
                    var parameter = parameters[i];
                    var parameterType = parameter.ParameterType;
                    if (CodeGenUtils.IsSpecialParameterType(parameterType))
                    {
                        // 剔除 JSContext, JSRuntime
                        ArrayUtility.RemoveAt(ref parameters, i);
                        len--;
                    }
                    // else if (parameter.IsOut)
                    // {
                    //     ArrayUtility.RemoveAt(ref parameters, i);
                    //     len--;
                    //     refParameters.Add(parameter);
                    // }
                    else
                    {
                        // if (parameterType.IsByRef)
                        // {
                        //     refParameters.Add(parameter);
                        // }
                        i++;
                    }
                }

                for (int i = 0, len = parameters.Length; i < len; i++)
                {
                    var parameter = parameters[i];
                    var parameter_prefix = "";
                    var parameterType = parameter.ParameterType;

                    if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && i == parameters.Length - 1)
                    {
                        var elementType = parameterType.GetElementType();
                        var elementTS = this.cg.currentTSModule.GetTSTypeFullName(elementType);
                        var parameterVarName = this.cg.bindingManager.GetTSVariable(parameter);
                        this.cg.tsDeclare.AppendL($"{parameter_prefix}...{parameterVarName}: {elementTS}[]");
                    }
                    else
                    {
                        var parameterTS = this.cg.currentTSModule.GetTSTypeFullName(parameter.ParameterType, parameter.IsOut);
                        var parameterVarName = this.cg.bindingManager.GetTSVariable(parameter);
                        this.cg.tsDeclare.AppendL($"{parameter_prefix}{parameterVarName}: {parameterTS}");
                    }

                    if (i != parameters.Length - 1)
                    {
                        this.cg.tsDeclare.AppendL(", ");
                    }
                }
                this.cg.tsDeclare.AppendL($")");
                WriteTSReturn(method, refParameters);
            }

            return refParameters;
        }

        // 获取返回值类型
        protected abstract Type GetReturnType(T method);

        // 获取方法调用
        protected abstract string GetInvokeBinding(string caller, T method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters);

        protected virtual void BeginInvokeBinding() { }

        protected virtual void EndInvokeBinding() { }

        protected virtual void OnBeforeExceptionReturn() { }

        protected void SplitParamters(ParameterInfo[] parameters, int index, List<ParameterInfo> out_params)
        {
            for (int i = index, len = parameters.Length; i < len; i++)
            {
                var p = parameters[i];

                if (p.IsOut || p.ParameterType.IsByRef)
                {
                    out_params.Add(p);
                }
            }
        }

        private void WriteRebindThis(MethodBase method, string caller)
        {
            if (!method.IsStatic && method.DeclaringType.IsValueType) // struct 非静态方法 检查 Mutable 属性
            {
                if (!string.IsNullOrEmpty(caller))
                {
                    cg.cs.AppendLine($"js_rebind_this(ctx, this_obj, ref {caller});");
                }
            }
        }

        // 写入绑定代码
        protected void WriteCSMethodBinding(MethodBaseBindingInfo<T> bindingInfo, T method, string argc, bool isVararg, bool isExtension)
        {
            if (this.cg.bindingManager.prefs.verboseLog)
            {
                cg.bindingManager.Info($"WriteCSMethodBinding: {method.Name} {isExtension}");
            }

            // 是否接管 cs 绑定代码生成
            var transform = cg.bindingManager.GetTypeTransform(method.DeclaringType);
            if (transform != null && transform.OnBinding(BindingPoints.METHOD_BINDING_FULL, method, cg))
            {
                return;
            }

            // var isRaw = method.IsDefined(typeof(JSCFunctionAttribute));
            var parameters = method.GetParameters();
            var caller = this.cg.AppendGetThisCS(method, isExtension);
            var returnType = GetReturnType(method);

            if (returnType == null || returnType == typeof(void))
            {
                // 方法本身没有返回值
                this.BeginInvokeBinding();
                cg.cs.AppendLine($"{this.GetInvokeBinding(caller, method, isVararg, isExtension, argc, parameters)};");
                this.EndInvokeBinding();

                _WriteBackParameters(isExtension, parameters);
                WriteRebindThis(method, caller);
                InvokeVoidReturn();
            }
            else
            {
                var retVar = "ret"; // cs return value var name
                this.BeginInvokeBinding();
                cg.cs.AppendLine($"var {retVar} = {this.GetInvokeBinding(caller, method, isVararg, isExtension, argc, parameters)};");
                this.EndInvokeBinding();

                var retPusher = cg.AppendMethodReturnValuePusher(method, returnType, retVar);

                _WriteBackParameters(isExtension, parameters);
                cg.cs.AppendLine("return {0};", retPusher);
            }
        }

        // 回填 ref/out 参数
        // 扩展方法参数索引需要偏移
        protected void _WriteBackParameters(bool isExtension, ParameterInfo[] parameters)
        {
            var pIndex = isExtension ? 1 : 0;
            var oIndex = 0;
            var pBase = pIndex;
            var needContext = true;
            for (; pIndex < parameters.Length; pIndex++)
            {
                var parameter = parameters[pIndex];
                var pType = parameter.ParameterType;

                if (!pType.IsByRef
                 || pType == typeof(Native.JSContext) || pType == typeof(Native.JSRuntime)
                 || pType == typeof(ScriptContext) || pType == typeof(ScriptRuntime))
                {
                    continue;
                }

                var baseIndex = pIndex - pBase;
                var pusher = cg.AppendValuePusher(parameter.ParameterType, $"arg{baseIndex}");

                cg.cs.AppendLine("var out{0} = {1};", oIndex, pusher);
                cg.cs.AppendLine("if (JSApi.JS_IsException(out{0}))", oIndex);
                using (cg.cs.CodeBlockScope())
                {
                    // for (var j = 0; j < oIndex; j++)
                    // {
                    //     cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, out{0});", j);
                    // }
                    OnBeforeExceptionReturn();
                    cg.cs.AppendLine("return out{0};", oIndex);
                }

                if (needContext)
                {
                    cg.cs.AppendLine("var context = ScriptEngine.GetContext(ctx);");
                    needContext = false;
                }

                cg.cs.AppendLine("JSApi.JS_SetProperty(ctx, argv[{0}], context.GetAtom(\"value\"), out{1});", baseIndex, oIndex);
                oIndex++;
            }
        }
    }

    public class ConstructorCodeGen : MethodBaseCodeGen<ConstructorInfo>
    {
        private TypeBindingInfo typeBindingInfo;
        private ConstructorBindingInfo bindingInfo;

        private bool disposable => typeBindingInfo.disposable;

        protected override Type GetReturnType(ConstructorInfo method)
        {
            return null;
        }

        // 写入默认构造函数 (struct 无参构造)
        private void WriteDefaultConstructorBinding()
        {
            var decalringTypeName = this.cg.bindingManager.GetCSTypeFullName(this.bindingInfo.decalringType);
            this.cg.cs.AppendLine("var o = new {0}();", decalringTypeName);
            this.cg.cs.AppendLine("var val = NewBridgeClassObject(ctx, new_target, o, magic, {0});", CodeGenUtils.ToLiteral(this.disposable));
            this.cg.cs.AppendLine("return val;");

            this.cg.tsDeclare.AppendLine($"{this.bindingInfo.jsName}()");
        }

        protected override string GetInvokeBinding(string caller, ConstructorInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters)
        {
            var arglist = Concat(AppendGetParameters(hasParams, nargs, parameters));
            var decalringTypeName = this.cg.bindingManager.GetCSTypeFullName(this.bindingInfo.decalringType);
            // 方法本身有返回值
            var transform = cg.bindingManager.GetTypeTransform(method.DeclaringType);
            if (transform == null || !transform.OnBinding(BindingPoints.METHOD_BINDING_BEFORE_INVOKE, method, cg, arglist))
            {
            }
            return $"var o = new {decalringTypeName}({arglist})";
        }

        protected override void EndInvokeBinding()
        {
            this.cg.cs.AppendLine("var val = NewBridgeClassObject(ctx, new_target, o, magic, {0});", CodeGenUtils.ToLiteral(this.disposable));
        }

        protected override void InvokeVoidReturn()
        {
            this.cg.cs.AppendLine("return val;");
        }

        protected override void OnBeforeExceptionReturn()
        {
            this.cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, val);");
        }

        public ConstructorCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo)
        : base(cg)
        {
            // WriteInstanceEvents(bindingInfo);
            this.typeBindingInfo = typeBindingInfo;
            this.bindingInfo = typeBindingInfo.constructors;
            if (this.bindingInfo.count > 0)
            {
                WriteCSAllVariants(this.bindingInfo);
                WriteTSAllVariants(typeBindingInfo, this.bindingInfo);
            }
            else
            {
                WriteDefaultConstructorBinding();
            }
        }

        // private void WriteInstanceEvents(TypeBindingInfo bindingInfo)
        // {
        //     var eventBindingInfos = new List<EventBindingInfo>();
        //     foreach (var kv in bindingInfo.events)
        //     {
        //         var eventBindingInfo = kv.Value;
        //         var bStatic = eventBindingInfo.isStatic;
        //         if (!bStatic)
        //         {
        //             eventBindingInfos.Add(eventBindingInfo);
        //         }
        //     }
        //     if (eventBindingInfos.Count > 0)
        //     {
        //         // Debug.Log($"Writing instance events... {bindingInfo.type}");
        //         this.cg.cs.AppendLine("DuktapeDLL.duk_push_this(ctx);");
        //         foreach (var eventBindingInfo in eventBindingInfos)
        //         {
        //             var tsFieldVar = this.cg.bindingManager.GetTSVariable(eventBindingInfo.regName);
        //             cg.cs.AppendLine($"duk_add_event(ctx, \"{tsFieldVar}\", {eventBindingInfo.adderName}, {eventBindingInfo.removerName}, -1);");
        //         }
        //         this.cg.cs.AppendLine("DuktapeDLL.duk_pop(ctx);");
        //     }
        // }
    }

    // 生成成员方法绑定代码
    public class MethodCodeGen : MethodBaseCodeGen<MethodInfo>
    {
        protected MethodBindingInfo methodBindingInfo;

        protected override Type GetReturnType(MethodInfo method)
        {
            return method.ReturnType;
        }

        private string WriteSetterBinding(string caller, ParameterInfo[] parameters)
        {
            var last = parameters.Length - 1;
            var arglist_t = "";
            var assignIndex = 0;
            for (var i = 0; i < last; i++)
            {
                var argname = $"arg{i}";
                if (this.WriteParameterGetter(parameters[i], assignIndex, argname))
                {
                    assignIndex++;
                }
                arglist_t += argname;
                if (i != last - 1)
                {
                    arglist_t += ", ";
                }
            }
            var argname_last = $"arg{last}";
            this.WriteParameterGetter(parameters[last], assignIndex, argname_last);
            return $"{caller}[{arglist_t}] = {argname_last}"; // setter
        }

        private string WriteGetterBinding(string caller, ParameterInfo[] parameters)
        {
            var last = parameters.Length;
            var arglist_t = "";
            var assignIndex = 0;
            for (var i = 0; i < last; i++)
            {
                var argname = $"arg{i}";
                if (this.WriteParameterGetter(parameters[i], assignIndex, argname))
                {
                    assignIndex++;
                }
                arglist_t += argname;
                if (i != last - 1)
                {
                    arglist_t += ", ";
                }
            }
            return $"{caller}[{arglist_t}]"; // getter
        }

        protected override string GetInvokeBinding(string caller, MethodInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters)
        {
            if (method.IsSpecialName) // if (bindingInfo.isIndexer)
            {
                if (method.Name == "set_Item") // if (method.ReturnType == typeof(void))
                {
                    return WriteSetterBinding(caller, parameters); // setter
                }
                else if (method.Name == "get_Item")
                {
                    return WriteGetterBinding(caller, parameters);
                }
            }

            var paramsToGet = isExtension ? parameters.Skip(1).ToArray() : parameters;
            var arglist = AppendGetParameters(hasParams, nargs, paramsToGet);
            var arglistSig = Concat(arglist);
            var transform = cg.bindingManager.GetTypeTransform(method.DeclaringType);

            // 在生成调用前插入代码 
            if (transform == null || !transform.OnBinding(BindingPoints.METHOD_BINDING_BEFORE_INVOKE, method, cg))
            {
            }

            // 扩展方法调用实际静态类方法
            if (isExtension)
            {
                var methodDeclType = this.cg.bindingManager.GetCSTypeFullName(method.DeclaringType);
                if (arglistSig.Length > 0)
                {
                    arglistSig = ", " + arglistSig;
                }

                return $"{methodDeclType}.{method.Name}({caller}{arglistSig})";
            }

            // 处理运算符方式调用
            if (method.IsSpecialName)
            {
                switch (method.Name)
                {
                    case "op_Modulus":
                        return $"{arglist[0]} % {arglist[1]}";
                    case "op_Equality":
                        return $"{arglist[0]} == {arglist[1]}";
                    case "op_Inequality":
                        return $"{arglist[0]} != {arglist[1]}";
                    case "op_LessThan":
                        return $"{arglist[0]} < {arglist[1]}";
                    case "op_LessThanOrEqual":
                        return $"{arglist[0]} <= {arglist[1]}";
                    case "op_GreaterThan":
                        return $"{arglist[0]} > {arglist[1]}";
                    case "op_GreaterThanOrEqual":
                        return $"{arglist[0]} >= {arglist[1]}";
                    case "op_Explicit":
                    case "op_Implicit":
                        var implicitTypeName = this.cg.bindingManager.GetCSTypeFullName(GetReturnType(method));
                        return $"({implicitTypeName}){arglist[0]}";
                }
            }

            // 普通成员调用
            return $"{caller}.{method.Name}({arglistSig})";
        }

        public MethodCodeGen(CodeGenerator cg, MethodBindingInfo bindingInfo)
        : base(cg)
        {
            this.methodBindingInfo = bindingInfo;
            WriteCSAllVariants(this.methodBindingInfo);
            // WriteTSAllVariants(this.bindingInfo);
        }
    }

    public class TSMethodCodeGen : MethodBaseCodeGen<MethodInfo>
    {
        protected MethodBindingInfo bindingInfo;

        protected override Type GetReturnType(MethodInfo method)
        {
            return method.ReturnType;
        }

        protected override string GetInvokeBinding(string caller, MethodInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters)
        {
            return null;
        }

        public TSMethodCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo, MethodBindingInfo bindingInfo)
            : base(cg)
        {
            this.bindingInfo = bindingInfo;
            WriteTSAllVariants(typeBindingInfo, this.bindingInfo);
        }
    }
}
