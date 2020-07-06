using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QuickJS.Editor
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
        public string GetFixedMatchTypes(T method)
        {
            var snippet = "";
            var parameters = method.GetParameters();
            var isExtension = method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
            var i = isExtension ? 1 : 0;
            var length = parameters.Length;
            for (; i < length; i++)
            {
                var parameter = parameters[i];
                if (parameter.IsDefined(typeof(ParamArrayAttribute), false))
                {
                    break;
                }
                snippet += ", ";
                if (parameter.ParameterType.IsByRef)
                {
                    //TODO: 检查 ref/out 参数有效性 (null undefined 或者 符合 Ref/Out 约定)
                    snippet += "null";
                }
                else
                {
                    var typename = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType);
                    snippet += $"typeof({typename})";
                }
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

        // parametersByRef: 可修改参数将被加入此列表
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
                    this.cg.cs.AppendLine("{");
                    this.cg.cs.AddTabLevel();
                    {
                        this.cg.cs.AppendLine($"arg{i} = new {argElementType}[{argElementIndex}];");
                        this.cg.cs.AppendLine($"for (var i = {i}; i < {nargs}; i++)");
                        this.cg.cs.AppendLine("{");
                        this.cg.cs.AddTabLevel();
                        {
                            var argElementOffset = i == 0 ? "" : " - " + i;
                            var argName = $"arg{i}[i{argElementOffset}]";
                            var argGetter = this.cg.bindingManager.GetScriptObjectGetter(parameter.ParameterType.GetElementType(), "ctx", "argv[i]", argName);
                            this.cg.cs.AppendLine("{0};", argGetter);
                        }
                        this.cg.cs.DecTabLevel();
                        this.cg.cs.AppendLine("}");
                    }
                    this.cg.cs.DecTabLevel();
                    this.cg.cs.AppendLine("}");
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

        // 对参数进行取值, 如果此参数无需取值, 则返回 false
        protected bool WriteParameterGetter(ParameterInfo parameter, int index, string argname)
        {
            var ptype = parameter.ParameterType;
            var argType = this.cg.bindingManager.GetCSTypeFullName(ptype);
            this.cg.cs.AppendLine($"{argType} {argname};");
            // 非 out 参数才需要取值
            if (!parameter.IsOut || !parameter.ParameterType.IsByRef)
            {
                var getter = this.cg.bindingManager.GetScriptObjectGetter(ptype, "ctx", $"argv[{index}]", argname);
                this.cg.cs.AppendLine("if (!{0})", getter);
                using (this.cg.cs.Block())
                {
                    this.cg.cs.AppendLine("throw new ParameterException(typeof({0}), {1});", argType, index);
                }
                return true;
            }

            return false;
        }

        // 输出所有变体绑定
        // hasOverrides: 是否需要处理重载
        protected void WriteAllVariants(MethodBaseBindingInfo<T> bindingInfo) // SortedDictionary<int, MethodBaseVariant<T>> variants)
        {
            var variants = bindingInfo.variants;
            var hasOverrides = bindingInfo.count > 1;
            if (hasOverrides)
            {
                // 需要处理重载
                GenMethodVariants(bindingInfo, variants);
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
                        // Debug.Log($"varargMethods {method}");
                        WriteCSMethodBinding(bindingInfo, method, argc, true);
                    }
                    else
                    {
                        var method = variant.plainMethods[0];
                        // Debug.Log($"plainMethods {method}");
                        WriteCSMethodBinding(bindingInfo, method, argc, false);
                    }
                }
            }
        }

        protected void WriteTSAllVariants(MethodBaseBindingInfo<T> bindingInfo)
        {
            var variants = bindingInfo.variants;
            //NOTE: 如果产生了无法在 typescript 中声明的方法, 则作标记, 并输出一条万能声明 
            //      [key: string]: any
            foreach (var variantKV in variants)
            {
                foreach (var method in variantKV.Value.plainMethods)
                {
                    WriteTSDeclaration(method, bindingInfo);
                }
                foreach (var method in variantKV.Value.varargMethods)
                {
                    WriteTSDeclaration(method, bindingInfo);
                }
            }
        }

        // 写入返回类型声明
        protected virtual void WriteTSReturn(T method, List<ParameterInfo> returnParameters)
        {
            var returnType = GetReturnType(method);
            var count = returnParameters.Count;
            if (returnType != null && returnType != typeof(void))
            {
                if (count != 0)
                {
                    this.cg.tsDeclare.AppendL(": { ");

                    var returnTypeTS = this.cg.bindingManager.GetTSTypeFullName(returnType);
                    var returnVarName = BindingManager.GetTSVariable("return");
                    this.cg.tsDeclare.AppendL($"\"{returnVarName}\": {returnTypeTS}");

                    for (var i = 0; i < count; i++)
                    {
                        var rp = returnParameters[i];
                        var name = BindingManager.GetTSVariable(rp.Name);
                        var ts = this.cg.bindingManager.GetTSTypeFullName(rp.ParameterType);
                        if (i != count - 1)
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
                    var returnTypeTS = this.cg.bindingManager.GetTSTypeFullName(returnType);
                    this.cg.tsDeclare.AppendL($": {returnTypeTS}");
                    this.cg.tsDeclare.AppendLine();
                }
            }
            else
            {
                if (count != 0)
                {
                    this.cg.tsDeclare.AppendL(": { ");
                    for (var i = 0; i < count; i++)
                    {
                        var rp = returnParameters[i];
                        var name = rp.Name;
                        var ts = this.cg.bindingManager.GetTSTypeFullName(rp.ParameterType);
                        if (i != count - 1)
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
                    this.cg.tsDeclare.AppendLine();
                }
            }
        }

        protected void GenMethodVariants(MethodBaseBindingInfo<T> bindingInfo, SortedDictionary<int, MethodBaseVariant<T>> variants)
        {
            var argc = cg.AppendGetArgCount(true);
            cg.cs.AppendLine("do");
            cg.cs.AppendLine("{");
            cg.cs.AddTabLevel();
            {
                foreach (var variantKV in variants)
                {
                    var args = variantKV.Key;
                    var variant = variantKV.Value;
                    //variant.count > 1
                    var gecheck = args > 0 && variant.isVararg; // 最后一组分支且存在变参时才需要判断 >= 
                    if (gecheck)
                    {
                        cg.cs.AppendLine("if (argc >= {0})", args);
                        cg.cs.AppendLine("{");
                        cg.cs.AddTabLevel();
                    }
                    // 处理定参
                    if (variant.plainMethods.Count > 0)
                    {
                        cg.cs.AppendLine("if (argc == {0})", args);
                        cg.cs.AppendLine("{");
                        cg.cs.AddTabLevel();
                        if (variant.plainMethods.Count > 1)
                        {
                            foreach (var method in variant.plainMethods)
                            {
                                cg.cs.AppendLine($"if (js_match_types(ctx, argv{GetFixedMatchTypes(method)}))");
                                cg.cs.AppendLine("{");
                                cg.cs.AddTabLevel();
                                this.WriteCSMethodBinding(bindingInfo, method, argc, false);
                                cg.cs.DecTabLevel();
                                cg.cs.AppendLine("}");
                            }
                            cg.cs.AppendLine("break;");
                        }
                        else
                        {
                            // 只有一个定参方法时, 不再判定类型匹配
                            var method = variant.plainMethods[0];
                            this.WriteCSMethodBinding(bindingInfo, method, argc, false);
                        }
                        cg.cs.DecTabLevel();
                        cg.cs.AppendLine("}");
                    }
                    // 处理变参
                    if (variant.varargMethods.Count > 0)
                    {
                        foreach (var method in variant.varargMethods)
                        {
                            cg.cs.AppendLine($"if (js_match_types(ctx, argv{GetFixedMatchTypes(method)})");
                            cg.cs.AppendLine($" && js_match_param_types(ctx, {args}, argv, {GetParamArrayMatchType(method)}))");
                            cg.cs.AppendLine("{");
                            cg.cs.AddTabLevel();
                            this.WriteCSMethodBinding(bindingInfo, method, argc, true);
                            cg.cs.DecTabLevel();
                            cg.cs.AppendLine("}");
                        }
                    }
                    if (gecheck)
                    {
                        cg.cs.DecTabLevel();
                        cg.cs.AppendLine("}");
                    }
                }
            }
            cg.cs.DecTabLevel();
            cg.cs.AppendLine("} while(false);");
            var error = this.cg.bindingManager.GetThrowError("no matched method variant");
            cg.cs.AppendLine($"return {error};");
        }

        protected List<ParameterInfo> WriteTSDeclaration(T method, MethodBaseBindingInfo<T> bindingInfo)
        {
            var isExtension = method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
            var refParameters = new List<ParameterInfo>();
            string tsMethodDeclaration;
            this.cg.AppendJSDoc(method);
            if (this.cg.bindingManager.GetTSMethodDeclaration(method, out tsMethodDeclaration))
            {
                this.cg.tsDeclare.AppendLine(tsMethodDeclaration);
                return refParameters;
            }
            var isRaw = method.IsDefined(typeof(JSCFunctionAttribute));
            //TODO: 需要处理参数类型归并问题, 因为如果类型没有导入 ts 中, 可能会在声明中出现相同参数列表的定义
            //      在 MethodVariant 中创建每个方法对应的TS类型名参数列表, 完全相同的不再输出
            var prefix = "";
            if (method.Name.StartsWith("op_"))
            {
                prefix += "//";
            }
            if (method.IsStatic && !isExtension)
            {
                prefix += "static ";
            }
            string tsMethodRename;
            if (this.cg.bindingManager.GetTSMethodRename(method, out tsMethodRename))
            {
                this.cg.tsDeclare.Append($"{prefix}{tsMethodRename}(");
            }
            else
            {
                this.cg.tsDeclare.Append($"{prefix}{bindingInfo.regName}(");
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

                // 剔除 out 参数
                for (int i = 0, len = parameters.Length; i < len;)
                {
                    var parameter = parameters[i];
                    if (parameter.IsOut)
                    {
                        ArrayUtility.RemoveAt(ref parameters, i);
                        len--;
                        refParameters.Add(parameter);
                    }
                    else
                    {
                        if (parameter.ParameterType.IsByRef)
                        {
                            refParameters.Add(parameter);
                        }
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
                        var elementTS = this.cg.bindingManager.GetTSTypeFullName(elementType);
                        var parameterVarName = BindingManager.GetTSVariable(parameter.Name);
                        this.cg.tsDeclare.AppendL($"{parameter_prefix}...{parameterVarName}: {elementTS}[]");
                    }
                    else
                    {
                        var parameterTS = this.cg.bindingManager.GetTSTypeFullName(parameterType, parameter.IsOut);
                        var parameterVarName = BindingManager.GetTSVariable(parameter.Name);
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

        // 写入无返回值的C#方法的返回代码
        protected virtual void InvokeVoidReturn()
        {
            cg.cs.AppendLine("return JSApi.JS_UNDEFINED;");
        }

        protected void SplitParamters(ParameterInfo[] parameters, int index, List<ParameterInfo> in_params, List<ParameterInfo> out_params)
        {
            for (int i = index, len = parameters.Length; i < len; i++)
            {
                var p = parameters[i];

                if (p.IsOut)
                {
                    out_params.Add(p);
                }
                else
                {
                    in_params.Add(p);
                }
            }
        }

        // 写入绑定代码
        protected void WriteCSMethodBinding(MethodBaseBindingInfo<T> bindingInfo, T method, string argc, bool isVararg)
        {
            // 是否接管 cs 绑定代码生成
            var transform = cg.bindingManager.GetTypeTransform(method.DeclaringType);
            if (transform != null && transform.OnBinding(BindingPoints.METHOD_BINDING_FULL, method, cg))
            {
                return;
            }

            var isExtension = method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
            var isRaw = method.IsDefined(typeof(JSCFunctionAttribute));
            var parameters = method.GetParameters();
            var in_params = new List<ParameterInfo>();
            var out_params = new List<ParameterInfo>();

            SplitParamters(parameters, isExtension ? 1 : 0, in_params, out_params);
            var caller = this.cg.AppendGetThisCS(method);
            var returnType = GetReturnType(method);

            if (isRaw)
            {
                do
                {
                    if (!isExtension)
                    {
                        if (returnType == typeof(int) && in_params.Count == 1)
                        {
                            var p = in_params[0];
                            if (p.ParameterType == typeof(IntPtr) && !p.IsOut)
                            {
                                cg.cs.AppendLine($"return {caller}.{method.Name}(ctx);");
                                return;
                            }
                        }
                        cg.bindingManager.Error($"invalid JSCFunction definition: {method}");
                        break;
                    }
                    cg.bindingManager.Error($"Extension as JSCFunction is not supported: {method}");
                } while (false);
            }

            if (returnType == null || returnType == typeof(void))
            {
                // 方法本身没有返回值
                this.BeginInvokeBinding();
                cg.cs.AppendLine($"{this.GetInvokeBinding(caller, method, isVararg, isExtension, argc, parameters)};");
                this.EndInvokeBinding();
                var backVars = out_params.Count > 0 ? _WriteBackParametersByRef(isExtension, out_params, null) : null;

                if (!method.IsStatic && method.DeclaringType.IsValueType) // struct 非静态方法 检查 Mutable 属性
                {
                    if (!string.IsNullOrEmpty(caller))
                    {
                        cg.cs.AppendLine($"js_rebind_this(ctx, this_obj, {caller});");
                    }
                }

                if (string.IsNullOrEmpty(backVars))
                {
                    this.InvokeVoidReturn();
                }
                else
                {
                    cg.cs.AppendLine("return {0};", backVars);
                }
            }
            else
            {
                var retVar = "ret"; // cs return value var name
                var retJsVar = "ret_js";
                this.BeginInvokeBinding();
                cg.cs.AppendLine($"var {retVar} = {this.GetInvokeBinding(caller, method, isVararg, isExtension, argc, parameters)};");
                this.EndInvokeBinding();

                var retPusher = cg.AppendMethodReturnValuePusher(method, returnType, retVar);
                cg.cs.AppendLine("var {0} = {1};", retJsVar, retPusher);
                cg.cs.AppendLine("if (JSApi.JS_IsException({0}))", retJsVar);
                using (cg.cs.Block())
                {
                    cg.cs.AppendLine("return {0};", retJsVar);
                }
                var backVars = out_params.Count > 0 ? _WriteBackParametersByRef(isExtension, out_params, retJsVar) : retJsVar;
                cg.cs.AppendLine("return {0};", backVars);
            }
        }

        // 回填 ref/out 参数
        // 扩展方法参数索引需要偏移
        protected string _WriteBackParametersByRef(bool isExtension, List<ParameterInfo> parametersByRef, string retJsVar)
        {
            var retVar = "mult_ret";
            cg.cs.AppendLine("var context = ScriptEngine.GetContext(ctx);");
            cg.cs.AppendLine("var {0} = JSApi.JS_NewObject(ctx);", retVar);

            if (!string.IsNullOrEmpty(retJsVar))
            {
                var retJsVarName = BindingManager.GetTSVariable("return");
                cg.cs.AppendLine("JSApi.JS_SetProperty(ctx, {0}, context.GetAtom(\"{1}\"), {2});", retVar, retJsVarName, retJsVar);
            }

            for (var i = 0; i < parametersByRef.Count; i++)
            {
                var parameter = parametersByRef[i];
                var pname = BindingManager.GetTSVariable(parameter.Name);
                var position = isExtension ? parameter.Position - 1 : parameter.Position;
                var argname = $"arg{position}";
                var pusher = cg.AppendValuePusher(parameter.ParameterType, argname);

                cg.cs.AppendLine("var out{0} = {1};", i, pusher);
                cg.cs.AppendLine("if (JSApi.JS_IsException(out{0}))", i);
                using (cg.cs.Block())
                {
                    for (var j = 0; j < i; j++)
                    {
                        cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, out{0});", j);
                    }
                    cg.cs.AppendLine("JSApi.JS_FreeValue(ctx, {0});", retVar);
                    cg.cs.AppendLine("return out{0};", i);
                }
                cg.cs.AppendLine("JSApi.JS_SetProperty(ctx, {0}, context.GetAtom(\"{1}\"), out{2});", retVar, pname, i);
            }
            return retVar;
        }
    }

    public class ConstructorCodeGen : MethodBaseCodeGen<ConstructorInfo>
    {
        private ConstructorBindingInfo bindingInfo;

        protected override Type GetReturnType(ConstructorInfo method)
        {
            return null;
        }

        // 写入默认构造函数 (struct 无参构造)
        private void WriteDefaultConstructorBinding()
        {
            var decalringTypeName = this.cg.bindingManager.GetCSTypeFullName(this.bindingInfo.decalringType);
            this.cg.cs.AppendLine("var o = new {0}();", decalringTypeName);
            this.cg.cs.AppendLine("var val = NewBridgeClassObject(ctx, new_target, o, magic);");
            this.cg.cs.AppendLine("return val;");

            this.cg.tsDeclare.AppendLine($"{this.bindingInfo.regName}()");
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
            this.cg.cs.AppendLine("var val = NewBridgeClassObject(ctx, new_target, o, magic);");
        }

        protected override void InvokeVoidReturn()
        {
            this.cg.cs.AppendLine("return val;");
        }

        public ConstructorCodeGen(CodeGenerator cg, TypeBindingInfo bindingInfo)
        : base(cg)
        {
            // WriteInstanceEvents(bindingInfo);
            this.bindingInfo = bindingInfo.constructors;
            if (this.bindingInfo.count > 0)
            {
                WriteAllVariants(this.bindingInfo);
                WriteTSAllVariants(this.bindingInfo);
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
        //             var tsFieldVar = BindingManager.GetTSVariable(eventBindingInfo.regName);
        //             cg.cs.AppendLine($"duk_add_event(ctx, \"{tsFieldVar}\", {eventBindingInfo.adderName}, {eventBindingInfo.removerName}, -1);");
        //         }
        //         this.cg.cs.AppendLine("DuktapeDLL.duk_pop(ctx);");
        //     }
        // }
    }

    // 生成成员方法绑定代码
    public class MethodCodeGen : MethodBaseCodeGen<MethodInfo>
    {
        protected MethodBindingInfo bindingInfo;

        protected override Type GetReturnType(MethodInfo method)
        {
            return method.ReturnType;
        }

        protected override string GetInvokeBinding(string caller, MethodInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters)
        {
            if (bindingInfo.isIndexer)
            {
                if (method.ReturnType == typeof(void))
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
                else
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
            }
            var arglist = Concat(AppendGetParameters(hasParams, nargs, parameters));
            var transform = cg.bindingManager.GetTypeTransform(method.DeclaringType);
           
            if (transform == null || !transform.OnBinding(BindingPoints.METHOD_BINDING_BEFORE_INVOKE, method, cg))
            {
            }

            if (isExtension)
            {
                var methodDeclType = this.cg.bindingManager.GetCSTypeFullName(method.DeclaringType);
                if (arglist.Length > 0)
                {
                    arglist = ", " + arglist;
                }

                return $"{methodDeclType}.{method.Name}({caller}{arglist})";
            }

            return $"{caller}.{method.Name}({arglist})";
        }

        public MethodCodeGen(CodeGenerator cg, MethodBindingInfo bindingInfo)
        : base(cg)
        {
            this.bindingInfo = bindingInfo;
            WriteAllVariants(this.bindingInfo);
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

        public TSMethodCodeGen(CodeGenerator cg, MethodBindingInfo bindingInfo)
            : base(cg)
        {
            this.bindingInfo = bindingInfo;
            WriteTSAllVariants(this.bindingInfo);
        }
    }
}
