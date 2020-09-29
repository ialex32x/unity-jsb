using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    // 所有具有相同参数数量的方法变体 (最少参数的情况下)
    public class MethodBaseVariant<T>
        where T : MethodBase
    {
        public int argc; // 最少参数数要求
        public List<T> plainMethods = new List<T>();
        public List<T> varargMethods = new List<T>();

        // 是否包含变参方法
        public bool isVararg
        {
            get { return varargMethods.Count > 0; }
        }

        public int count
        {
            get { return plainMethods.Count + varargMethods.Count; }
        }

        public MethodBaseVariant(int argc)
        {
            this.argc = argc;
        }

        public bool CheckMethodEquality(ParameterInfo[] a, int aIndex, ParameterInfo[] b, int bIndex)
        {
            var aLen = a.Length - aIndex;
            var bLen = b.Length - bIndex;
            if (aLen != bLen)
            {
                return false;
            }

            while (aIndex < a.Length && bIndex < b.Length)
            {
                var aInfo = a[aIndex++];
                var bInfo = b[bIndex++];
                if (aInfo.ParameterType != bInfo.ParameterType || aInfo.IsOut != bInfo.IsOut)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CheckMethodEquality(T a, T b)
        {
            return CheckMethodEquality(
                a.GetParameters(), BindingManager.IsExtensionMethod(a) ? 1 : 0,
                b.GetParameters(), BindingManager.IsExtensionMethod(b) ? 1 : 0
            );
        }

        public void Add(T methodInfo, bool isVararg)
        {
            //TODO: method 按照参数的具体程度排序以提高 match_type 的有效命中率
            if (isVararg)
            {
                foreach (var entry in this.varargMethods)
                {
                    if (CheckMethodEquality(entry, methodInfo))
                    {
                        return;
                    }
                }
                this.varargMethods.Add(methodInfo);
            }
            else
            {
                foreach (var entry in this.plainMethods)
                {
                    if (CheckMethodEquality(entry, methodInfo))
                    {
                        return;
                    }
                }
                this.plainMethods.Add(methodInfo);
            }
        }
    }
}