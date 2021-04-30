using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public interface IBindingUtils
    {
        string ReplacePathVars(string value);

        bool IsExplicitEditorType(Type type);
    }

    public class DefaultBindingUtils : IBindingUtils
    {
        public string ReplacePathVars(string value)
        {
#if JSB_UNITYLESS
            value = value.Replace("${platform}", "unityless");
            return value;
#else
            return Unity.UnityHelper.ReplacePathVars(value);
#endif
        }

        public bool IsExplicitEditorType(Type type)
        {
#if JSB_UNITYLESS
            return false;
#else
            return Unity.UnityHelper.IsExplicitEditorType(type);
#endif
        }
    }
}