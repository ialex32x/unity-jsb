using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class PropertyGetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected PropertyBindingInfo bindingInfo;

        public PropertyGetterCodeGen(CodeGenerator cg, PropertyBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var caller = this.cg.AppendGetThisCS(bindingInfo.propertyInfo.GetMethod);

            this.cg.cs.AppendLine("var ret = {0}.{1};", caller, bindingInfo.propertyInfo.Name);
            this.cg.AppendPushValue(bindingInfo.propertyInfo.PropertyType, "ret");
            this.cg.cs.AppendLine("return 1;");
        }

        public virtual void Dispose()
        {
        }
    }

    public class PropertySetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected PropertyBindingInfo bindingInfo;

        public PropertySetterCodeGen(CodeGenerator cg, PropertyBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var method = bindingInfo.propertyInfo.SetMethod;
            var propertyInfo = this.bindingInfo.propertyInfo;
            var declaringType = propertyInfo.DeclaringType;

            var caller = this.cg.AppendGetThisCS(method);
            this.cg.cs.AppendLine("{0} value;", this.cg.bindingManager.GetCSTypeFullName(propertyInfo.PropertyType));
            this.cg.cs.AppendLine(this.cg.bindingManager.GetDuktapeGetter(propertyInfo.PropertyType, "ctx", "0", "value"));
            this.cg.cs.AppendLine("{0}.{1} = value;", caller, propertyInfo.Name);
            if (declaringType.IsValueType && !method.IsStatic)
            {
                // 非静态结构体属性修改, 尝试替换实例
                this.cg.cs.AppendLine($"duk_rebind_this(ctx, {caller});");
            }
            this.cg.cs.AppendLine("return 0;");
        }

        public virtual void Dispose()
        {
        }
    }
}
