using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class DelegateBridgeBindingInfo
    {
        public HashSet<Type> types = new HashSet<Type>();
        public Type returnType;
        public ParameterInfo[] parameters;
        public bool isEditorRuntime;

        public DelegateBridgeBindingInfo(Type returnType, ParameterInfo[] parameters, bool isEditorRuntime)
        {
            this.returnType = returnType;
            this.parameters = parameters;
            this.isEditorRuntime = isEditorRuntime;
        }

        public bool Equals(Type returnType, ParameterInfo[] parameters, bool isEditorRuntime)
        {
            if (this.isEditorRuntime != isEditorRuntime)
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
