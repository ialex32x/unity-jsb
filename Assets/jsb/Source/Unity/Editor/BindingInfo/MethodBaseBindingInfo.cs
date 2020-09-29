using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    
    public abstract class MethodBaseBindingInfo<T>
        where T : MethodBase
    {
        public string csBindName { get; set; } // 绑定代码名
        public string jsName { get; set; } // 导出名

        private int _count = 0;

        // 按照参数数逆序排序所有变体
        // 有相同参数数量要求的方法记录在同一个 Variant 中 (变参方法按最少参数数计算, 不计变参参数数)
        public SortedDictionary<int, MethodBaseVariant<T>> variants = new SortedDictionary<int, MethodBaseVariant<T>>(new MethodVariantComparer());

        // 标记为 JSCFunction, 不生成包装代码, 直接注册给JS
        // 必须为静态函数, 且函数签名完全匹配 JSCFunction
        public MethodBase _cfunc;

        public int count
        {
            get { return _count; }
        }

        public static bool IsVarargMethod(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        public static int GetTSParameterCount(ParameterInfo[] parameters)
        {
            var len = parameters.Length;
            var argc = len;
            // for (var i = 0; i < len; i++)
            // {
            //     var p = parameters[i];
            //     if (p.IsOut)
            //     {
            //         argc--;
            //     }
            // }
            return argc;
        }

        public bool Add(T method, bool isExtension)
        {
            if (method.IsDefined(typeof(JSCFunctionAttribute)))
            {
                if (!method.IsStatic || _cfunc != null)
                {
                    return false;
                }
                this._cfunc = method;
                return true;
            }

            var parameters = method.GetParameters();
            var nargs = GetTSParameterCount(parameters);
            var isVararg = IsVarargMethod(parameters);
            MethodBaseVariant<T> variant;
            if (isVararg)
            {
                nargs--;
            }

            if (isExtension)
            {
                nargs--;
            }

            if (!this.variants.TryGetValue(nargs, out variant))
            {
                variant = new MethodBaseVariant<T>(nargs);
                this.variants.Add(nargs, variant);
            }

            _count++;
            variant.Add(method, isVararg);
            return true;
        }
    }

    public class MethodBindingInfo : MethodBaseBindingInfo<MethodInfo>
    {
        public MethodBindingInfo(bool bStatic, string csName, string jsName)
        {
            this.csBindName = (bStatic ? "BindStatic_" : "Bind_") + csName;
            this.jsName = jsName;
        }
    }

    public class OperatorBindingInfo : MethodBaseBindingInfo<MethodInfo>
    {
        public int length; // 参数数
        public string csName;
        public string cs_op; // 绑定代码中的运算符
        public MethodInfo methodInfo;
        public bool isExtension;

        // regName: js 中的重载运算符
        public OperatorBindingInfo(MethodInfo methodInfo, bool isExtension, bool bStatic, string csName, string jsName, string cs_op, int length)
        {
            this.methodInfo = methodInfo;
            this.isExtension = isExtension;
            this.length = length;
            this.csName = csName;
            this.jsName = jsName;
            this.cs_op = cs_op;
            this.csBindName = (bStatic ? "BindStatic_" : "Bind_") + csName;

            this.Add(methodInfo, isExtension); //NOTE: 旧代码, 待更替
        }
    }

    public class ConstructorBindingInfo : MethodBaseBindingInfo<ConstructorInfo>
    {
        public Type decalringType;

        // public 构造是否可用
        public bool available
        {
            get
            {
                if (decalringType.IsValueType && !decalringType.IsPrimitive && !decalringType.IsAbstract)
                {
                    return true; // default constructor for struct
                }

                return variants.Count > 0;
            }
        }

        public ConstructorBindingInfo(Type decalringType)
        {
            this.decalringType = decalringType;
            this.csBindName = "BindConstructor";
            this.jsName = "constructor";
        }
    }

}