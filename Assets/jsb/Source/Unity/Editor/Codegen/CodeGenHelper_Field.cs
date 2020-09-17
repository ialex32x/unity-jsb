using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class FieldGetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected FieldBindingInfo bindingInfo;

        public FieldGetterCodeGen(CodeGenerator cg, FieldBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var caller = this.cg.AppendGetThisCS(bindingInfo);

            this.cg.cs.AppendLine("var ret = {0}.{1};", caller, bindingInfo.fieldInfo.Name);
            var pusher = this.cg.AppendValuePusher(bindingInfo.fieldInfo.FieldType, "ret");
            this.cg.cs.AppendLine("return {0};", pusher);
        }

        public virtual void Dispose()
        {
        }
    }

    public class FieldSetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected FieldBindingInfo bindingInfo;

        public FieldSetterCodeGen(CodeGenerator cg, FieldBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var caller = this.cg.AppendGetThisCS(bindingInfo);
            var fieldInfo = bindingInfo.fieldInfo;
            var declaringType = fieldInfo.DeclaringType;
            var fieldTypeName = this.cg.bindingManager.GetCSTypeFullName(fieldInfo.FieldType);
            this.cg.cs.AppendLine("{0} value;", fieldTypeName);
            var getter = this.cg.bindingManager.GetScriptObjectGetter(fieldInfo.FieldType, "ctx", "arg_val", "value");
            this.cg.cs.AppendLine("if (!{0})", getter);
            using (this.cg.cs.CodeBlockScope())
            {
                this.cg.cs.AppendLine("throw new ParameterException(typeof({0}), 0);", fieldTypeName);
            }
            this.cg.cs.AppendLine("{0}.{1} = value;", caller, fieldInfo.Name);
            if (declaringType.IsValueType && !fieldInfo.IsStatic)
            {
                // 非静态结构体字段修改, 尝试替换实例
                this.cg.cs.AppendLine($"js_rebind_this(ctx, this_obj, ref {caller});");
            }
            this.cg.cs.AppendLine("return JSApi.JS_UNDEFINED;");
        }

        public virtual void Dispose()
        {
        }
    }
}
