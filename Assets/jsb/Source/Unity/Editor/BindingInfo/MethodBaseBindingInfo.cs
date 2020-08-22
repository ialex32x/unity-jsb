using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    
    public abstract class MethodBaseBindingInfo<T>
        where T : MethodBase
    {
        public string name { get; set; } // 绑定代码名
        public string regName { get; set; } // 导出名

        private int _count = 0;

        // 按照参数数逆序排序所有变体
        // 有相同参数数量要求的方法记录在同一个 Variant 中 (变参方法按最少参数数计算, 不计变参参数数)
        public SortedDictionary<int, MethodBaseVariant<T>> variants = new SortedDictionary<int, MethodBaseVariant<T>>(new MethodVariantComparer());
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
            for (var i = 0; i < len; i++)
            {
                var p = parameters[i];
                if (p.IsOut)
                {
                    argc--;
                }
            }
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
        public bool isIndexer;

        public MethodBindingInfo(bool isIndexer, bool bStatic, string bindName, string regName)
        {
            this.isIndexer = isIndexer;
            this.name = (bStatic ? "BindStatic_" : "Bind_") + bindName;
            this.regName = regName;
        }
    }

    public class OperatorBindingInfo : MethodBaseBindingInfo<MethodInfo>
    {
        public int length; // 参数数
        public string bindName;
        public string cs_op; // 绑定代码中的运算符
        public MethodInfo methodInfo;
        public bool isExtension;

        // regName: js 中的重载运算符
        public OperatorBindingInfo(MethodInfo methodInfo, bool isExtension, bool bStatic, string bindName, string regName, string cs_op, int length)
        {
            this.methodInfo = methodInfo;
            this.isExtension = isExtension;
            this.length = length;
            this.bindName = bindName;
            this.regName = regName;
            this.cs_op = cs_op;
            this.name = (bStatic ? "BindStatic_" : "Bind_") + bindName;

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
            this.name = "BindConstructor";
            this.regName = "constructor";
        }
    }

}