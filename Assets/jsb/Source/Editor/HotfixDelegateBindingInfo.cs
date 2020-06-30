using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class HotfixDelegateBindingInfo
    {
        public Type returnType;
        public Type thisType;
        public ParameterInfo[] parameters;

        public HotfixDelegateBindingInfo(Type thisType, Type returnType, ParameterInfo[] parameters)
        {
            this.thisType = thisType;
            this.returnType = returnType;
            this.parameters = parameters;
        }

        public bool Equals(Type thisType, Type returnType, ParameterInfo[] parameters)
        {
            if (thisType != this.thisType)
            {
                return false;
            }

            if (returnType != this.returnType || parameters.Length != this.parameters.Length)
            {
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != this.parameters[i].ParameterType)
                {
                    return false;
                }

                if (parameters[i].IsOut != this.parameters[i].IsOut)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
