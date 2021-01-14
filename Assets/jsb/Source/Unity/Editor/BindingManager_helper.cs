using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public partial class BindingManager
    {
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
                var parameterType = parameters[i].ParameterType;
                if (CodeGenUtils.IsSpecialParameterType(parameterType))
                {
                    argc--;
                }
            }
            return argc;
        }

        public static bool IsExtensionMethod(MethodBase method)
        {
            return method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
        }

        public static bool ContainsGenericParameters(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (int i = 0, size = parameters.Length; i < size; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsGenericTypeDefinition || parameterType.IsGenericParameter)
                {
                    return true;
                }
            }
            return false;
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
        public static bool ContainsByRefParameters(ParameterInfo[] parameters)
        {
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

        public static bool ContainsByRefParameters(MethodBase method)
        {
            return ContainsByRefParameters(method.GetParameters());
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
