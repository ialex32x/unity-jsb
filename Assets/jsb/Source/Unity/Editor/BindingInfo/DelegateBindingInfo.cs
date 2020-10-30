using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class DelegateBindingInfo
    {
        public string name = null; // 绑定代码名
        public string regName = null; // js 注册名
        public string csName = null; // cs 代码名

        public Type declaringType;
        public Type delegateType;
        public bool readable; // 可读
        public bool writable; // 可写
        public bool isStatic; // 静态

        public DelegateBindingInfo(TypeBindingInfo typeBindingInfo, FieldInfo fieldInfo)
        {
            this.declaringType = typeBindingInfo.type;
            this.delegateType = fieldInfo.FieldType;
            this.readable = true;
            this.writable = !fieldInfo.IsInitOnly;
            this.isStatic = fieldInfo.IsStatic;
            this.csName = fieldInfo.Name;

            do
            {
                if (this.isStatic)
                {
                    this.name = "BindStaticDelegate_" + fieldInfo.Name;
                }
                else
                {
                    this.name = "BindDelegate_" + fieldInfo.Name;
                }
            } while (false);

            this.regName = typeBindingInfo.bindingManager.GetNamingAttribute(fieldInfo);
        }

        public DelegateBindingInfo(TypeBindingInfo typeBindingInfo, PropertyInfo propertyInfo)
        {
            this.declaringType = typeBindingInfo.type;
            this.delegateType = propertyInfo.PropertyType;
            this.readable = propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic;
            this.writable = propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic;
            this.isStatic = (propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic;
            this.csName = propertyInfo.Name;

            if (this.isStatic)
            {
                this.name = "BindStaticDelegate_" + propertyInfo.Name;
            }
            else
            {
                this.name = "BindDelegate_" + propertyInfo.Name;
            }

            this.regName = typeBindingInfo.bindingManager.GetNamingAttribute(propertyInfo);
        }
    }
}
