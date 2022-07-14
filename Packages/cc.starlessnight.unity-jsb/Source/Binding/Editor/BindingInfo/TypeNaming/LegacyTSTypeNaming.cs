#if UNITY_EDITOR || JSB_RUNTIME_REFLECT_BINDING
using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    public class LegacyTSTypeNaming : ITSTypeNaming
    {
        public override void Initialize(BindingManager bindingManager, Type type)
        {
            this.moduleName = type.Namespace ?? "";
            var tsNaming = bindingManager.GetTypeTransform(type)?.GetTSNaming();

            if (tsNaming == null)
            {
                genericDefinition = "";

                // remove the suffix primitive array types
                tsNaming = type.IsArray ? type.Name.Substring(0, type.Name.Length - 2) : type.Name;
                var gArgIndex = tsNaming.IndexOf('`');
                if (gArgIndex >= 0)
                {
                    tsNaming = tsNaming.Substring(0, gArgIndex);
                }

                // flattening constructed generic type
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    foreach (var gp in type.GetGenericArguments())
                    {
                        tsNaming += "_" + gp.Name;
                    }
                }
            }
            else
            {
                var gArgIndex = tsNaming.IndexOf('<');
                if (gArgIndex >= 0)
                {
                    genericDefinition = tsNaming.Substring(gArgIndex);
                    tsNaming = tsNaming.Substring(0, gArgIndex);
                }
                else
                {
                    genericDefinition = "";
                }
            }

            var headingPath = new List<string>();

            // 处理内部类层级
            var declaringType = type.DeclaringType;
            while (declaringType != null)
            {
                headingPath.Insert(0, StripCSharpGenericDefinition(declaringType.Name));
                declaringType = declaringType.DeclaringType;
            }
            headingPath.Add(tsNaming);
            classPath = headingPath.ToArray();
        }
    }
}

#endif
