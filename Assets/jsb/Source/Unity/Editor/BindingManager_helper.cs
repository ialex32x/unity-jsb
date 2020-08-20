using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public partial class BindingManager
    {
        public static bool IsExtensionMethod(MethodBase method)
        {
            return method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
        }

        // 是否包含指针参数
        public static bool ContainsPointer(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (int i = 0, size = parameters.Length; i < size; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsPointer)
                {
                    return true;
                }
            }
            return false;
        }

        // 是否包含按引用传参 (ref/out)
        public static bool ContainsByRefParameters(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (int i = 0, size = parameters.Length; i < size; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsGenericMethod(MethodBase method)
        {
            return method.GetGenericArguments().Length > 0;
        }

        public static bool IsUnsupported(MethodBase method)
        {
            return ContainsPointer(method) || IsGenericMethod(method);
        }
    }
}
