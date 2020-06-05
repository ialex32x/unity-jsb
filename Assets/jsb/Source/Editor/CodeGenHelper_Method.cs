using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
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

        // parametersByRef: 可修改参数将被加入此列表
        // hasParams: 是否包含变参 (最后一个参数将按数组处理)
        public string AppendGetParameters(bool hasParams, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
        {
            var arglist = "";
            var argBase = 0;
            for (var i = argBase; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.IsOut && parameter.ParameterType.IsByRef)
                {
                    arglist += "out ";
                    if (parametersByRef != null)
                    {
                        parametersByRef.Add(parameter);
                    }
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    arglist += "ref ";
                    if (parametersByRef != null)
                    {
                        parametersByRef.Add(parameter);
                    }
                }
                arglist += "arg" + i;
                if (i != parameters.Length - 1)
                {
                    arglist += ", ";
                }
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
                            this.cg.cs.AppendLine(this.cg.bindingManager.GetDuktapeGetter(parameter.ParameterType.GetElementType(), "ctx", "i", argName));
                        }
                        this.cg.cs.DecTabLevel();
                        this.cg.cs.AppendLine("}");
                    }
                    this.cg.cs.DecTabLevel();
                    this.cg.cs.AppendLine("}");
                }
                else
                {
                    WriteParameterGetter(parameter, i, $"arg{i}");
                }
            }
            return arglist;
        }

        protected void WriteParameterGetter(ParameterInfo parameter, int index, string argname)
        {
            var ptype = parameter.ParameterType;
            var argType = this.cg.bindingManager.GetCSTypeFullName(ptype);
            this.cg.cs.AppendLine($"{argType} {argname};");
            // 非 out 参数才需要取值
            if (!parameter.IsOut || !parameter.ParameterType.IsByRef)
            {
                this.cg.cs.AppendLine(this.cg.bindingManager.GetDuktapeGetter(ptype, "ctx", index + "", argname));
            }
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
                GenMethodVariants(variants);
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
                        WriteCSMethodBinding(method, argc, true);
                    }
                    else
                    {
                        var method = variant.plainMethods[0];
                        // Debug.Log($"plainMethods {method}");
                        WriteCSMethodBinding(method, argc, false);
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
            if (returnType != null)
            {
                var returnTypeTS = this.cg.bindingManager.GetTSTypeFullName(returnType);
                this.cg.tsDeclare.AppendL($": {returnTypeTS}");
                this.cg.tsDeclare.AppendLine();
            }
            else
            {
                this.cg.tsDeclare.AppendLine();
            }
        }

        protected void GenMethodVariants(SortedDictionary<int, MethodBaseVariant<T>> variants)
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
                                cg.cs.AppendLine($"if (duk_match_types(ctx, argc{GetFixedMatchTypes(method)}))");
                                cg.cs.AppendLine("{");
                                cg.cs.AddTabLevel();
                                this.WriteCSMethodBinding(method, argc, false);
                                cg.cs.DecTabLevel();
                                cg.cs.AppendLine("}");
                            }
                            cg.cs.AppendLine("break;");
                        }
                        else
                        {
                            // 只有一个定参方法时, 不再判定类型匹配
                            var method = variant.plainMethods[0];
                            this.WriteCSMethodBinding(method, argc, false);
                        }
                        cg.cs.DecTabLevel();
                        cg.cs.AppendLine("}");
                    }
                    // 处理变参
                    if (variant.varargMethods.Count > 0)
                    {
                        foreach (var method in variant.varargMethods)
                        {
                            cg.cs.AppendLine($"if (duk_match_types(ctx, argc{GetFixedMatchTypes(method)})");
                            cg.cs.AppendLine($" && duk_match_param_types(ctx, {args}, argc, {GetParamArrayMatchType(method)}))");
                            cg.cs.AppendLine("{");
                            cg.cs.AddTabLevel();
                            this.WriteCSMethodBinding(method, argc, true);
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
            var error = this.cg.bindingManager.GetDuktapeGenericError("no matched method variant");
            cg.cs.AppendLine($"return {error}");
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
            if (method.IsStatic && !isExtension)
            {
                prefix = "static ";
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
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var parameter_prefix = "";
                    var parameterType = parameter.ParameterType;
                    if (parameter.IsOut && parameterType.IsByRef)
                    {
                        // parameter_prefix = "/*out*/ ";
                        refParameters.Add(parameter);
                    }
                    else if (parameterType.IsByRef)
                    {
                        // parameter_prefix = "/*ref*/ ";
                        refParameters.Add(parameter);
                    }
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
        protected abstract string GetInvokeBinding(string caller, T method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef);

        protected virtual void BeginInvokeBinding() { }

        protected virtual void EndInvokeBinding() { }

        // 写入绑定代码
        protected void WriteCSMethodBinding(T method, string argc, bool isVararg)
        {
            var parameters = method.GetParameters();
            var isExtension = method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
            var isRaw = method.IsDefined(typeof(JSCFunctionAttribute));
            if (isExtension)
            {
                ArrayUtility.RemoveAt(ref parameters, 0);
            }
            var parametersByRef = new List<ParameterInfo>();
            var caller = this.cg.AppendGetThisCS(method);
            var returnType = GetReturnType(method);

            if (isRaw)
            {
                do
                {
                    if (!isExtension)
                    {
                        if (returnType == typeof(int) && parameters.Length == 1)
                        {
                            var p = parameters[0];
                            if (p.ParameterType == typeof(IntPtr) && !p.IsOut)
                            {
                                cg.cs.AppendLine($"return {caller}.{method.Name}(ctx);");
                                return;
                            }
                        }
                        cg.bindingManager.Error($"Invalid JSCFunction definition: {method}");
                        break;
                    }
                    cg.bindingManager.Error($"Extension as JSCFunction is not supported: {method}");
                } while (false);
            }

            if (returnType == null || returnType == typeof(void))
            {
                // 方法本身没有返回值
                this.BeginInvokeBinding();
                cg.cs.AppendLine($"{this.GetInvokeBinding(caller, method, isVararg, isExtension, argc, parameters, parametersByRef)};");
                this.EndInvokeBinding();
                if (parametersByRef.Count > 0)
                {
                    _WriteBackParametersByRef(isExtension, parametersByRef);
                }
                if (!method.IsStatic && method.DeclaringType.IsValueType) // struct 非静态方法 检查 Mutable 属性
                {
                    if (!string.IsNullOrEmpty(caller))
                    {
                        cg.cs.AppendLine($"duk_rebind_this(ctx, {caller});");
                    }
                }
                cg.cs.AppendLine("return JSApi.JS_UNDEFINED;");
            }
            else
            {
                // 方法本身有返回值
                this.BeginInvokeBinding();
                cg.cs.AppendLine($"var ret = {this.GetInvokeBinding(caller, method, isVararg, isExtension, argc, parameters, parametersByRef)};");
                this.EndInvokeBinding();
                if (parametersByRef.Count > 0)
                {
                    _WriteBackParametersByRef(isExtension, parametersByRef);
                }
                cg.AppendPushValue(returnType, "ret");
                cg.cs.AppendLine("return 1;");
            }
        }

        // 回填 ref/out 参数
        // 扩展方法参数索引需要偏移
        protected void _WriteBackParametersByRef(bool isExtension, List<ParameterInfo> parametersByRef)
        {
            for (var i = 0; i < parametersByRef.Count; i++)
            {
                var parameter = parametersByRef[i];
                var position = isExtension ? parameter.Position - 1 : parameter.Position;
                cg.cs.AppendLine($"if (!DuktapeDLL.duk_is_null_or_undefined(ctx, {position}))");
                cg.cs.AppendLine("{");
                cg.cs.AddTabLevel();
                var argname = $"arg{position}";
                cg.AppendPushValue(parameter.ParameterType, argname);
                cg.cs.AppendLine($"DuktapeDLL.duk_unity_put_target_i(ctx, {position});");
                cg.cs.DecTabLevel();
                cg.cs.AppendLine("}");
            }
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

        protected override string GetInvokeBinding(string caller, ConstructorInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
        {
            var arglist = this.AppendGetParameters(hasParams, nargs, parameters, parametersByRef);
            var decalringTypeName = this.cg.bindingManager.GetCSTypeFullName(this.bindingInfo.decalringType);
            return $"var o = new {decalringTypeName}({arglist})";
        }

        protected override void EndInvokeBinding()
        {
            this.cg.cs.AppendLine("var val = NewBridgeClassObject(ctx, new_target, o, magic);");
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

        protected override string GetInvokeBinding(string caller, MethodInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
        {
            if (bindingInfo.isIndexer)
            {
                if (method.ReturnType == typeof(void))
                {
                    var last = parameters.Length - 1;
                    var arglist_t = "";
                    for (var i = 0; i < last; i++)
                    {
                        var argname = $"arg{i}";
                        this.WriteParameterGetter(parameters[i], i, argname);
                        arglist_t += argname;
                        if (i != last - 1)
                        {
                            arglist_t += ", ";
                        }
                    }
                    var argname_last = $"arg{last}";
                    this.WriteParameterGetter(parameters[last], last, argname_last);
                    return $"{caller}[{arglist_t}] = {argname_last}"; // setter
                }
                else
                {
                    var last = parameters.Length;
                    var arglist_t = "";
                    for (var i = 0; i < last; i++)
                    {
                        var argname = $"arg{i}";
                        this.WriteParameterGetter(parameters[i], i, argname);
                        arglist_t += argname;
                        if (i != last - 1)
                        {
                            arglist_t += ", ";
                        }
                    }
                    return $"{caller}[{arglist_t}]"; // getter
                }
            }
            var arglist = this.AppendGetParameters(hasParams, nargs, parameters, parametersByRef);
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

        protected override string GetInvokeBinding(string caller, MethodInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
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
