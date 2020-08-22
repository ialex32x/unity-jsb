using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Editor
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

        public void Add(T methodInfo, bool isVararg)
        {
            //TODO: method 按照参数的具体程度排序以提高 match_type 的有效命中率
            if (isVararg)
            {
                this.varargMethods.Add(methodInfo);
            }
            else
            {
                this.plainMethods.Add(methodInfo);
            }
        }
    }
}