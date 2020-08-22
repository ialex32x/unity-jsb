using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public struct PropertyBindingPair
    {
        public string getterName; // 绑定代码名
        public string setterName;

        public bool IsValid()
        {
            return this.getterName != null || this.setterName != null;
        }
    }

    public class PropertyBindingInfo
    {
        public PropertyBindingPair staticPair;

        public PropertyBindingPair instancePair;

        // public string getterName; // 绑定代码名
        // public string setterName;
        public string regName; // js 注册名
        public PropertyInfo propertyInfo;

        public PropertyBindingInfo(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            if (propertyInfo.CanRead && propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                if (propertyInfo.GetMethod.IsStatic)
                {
                    staticPair.getterName = "BindStaticRead_" + propertyInfo.Name;
                }
                else
                {
                    instancePair.getterName = "BindRead_" + propertyInfo.Name;
                }
            }

            if (propertyInfo.CanWrite && propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic)
            {
                if (propertyInfo.SetMethod.IsStatic)
                {
                    staticPair.setterName = "BindStaticWrite_" + propertyInfo.Name;
                }
                else
                {
                    instancePair.setterName = "BindWrite_" + propertyInfo.Name;
                }
            }

            this.regName = TypeBindingInfo.GetNamingAttribute(propertyInfo);
        }
    }

}