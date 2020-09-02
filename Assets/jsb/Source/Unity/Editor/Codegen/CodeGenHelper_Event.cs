using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class EventAdderCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected EventBindingInfo bindingInfo;

        public EventAdderCodeGen(CodeGenerator cg, EventBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var eventInfo = this.bindingInfo.eventInfo;
            var declaringType = eventInfo.DeclaringType;

            var caller = this.cg.AppendGetThisCS(bindingInfo);
            this.cg.cs.AppendLine("{0} value;", this.cg.bindingManager.GetCSTypeFullName(eventInfo.EventHandlerType));
            var getter = this.cg.bindingManager.GetScriptObjectGetter(eventInfo.EventHandlerType, "ctx", "argv[0]", "value");
            this.cg.cs.AppendLine("{0};", getter);
            this.cg.cs.AppendLine("{0}.{1} += value;", caller, eventInfo.Name);
            if (declaringType.IsValueType && !eventInfo.GetAddMethod().IsStatic)
            {
                // 非静态结构体属性修改, 尝试替换实例
                this.cg.cs.AppendLine($"js_rebind_this(ctx, this_obj, {caller});");
            }
            this.cg.cs.AppendLine("return JSApi.JS_UNDEFINED;");
        }

        public virtual void Dispose()
        {
        }
    }

    public class EventRemoverCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected EventBindingInfo bindingInfo;

        public EventRemoverCodeGen(CodeGenerator cg, EventBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var eventInfo = this.bindingInfo.eventInfo;
            var declaringType = eventInfo.DeclaringType;

            var caller = this.cg.AppendGetThisCS(bindingInfo);
            this.cg.cs.AppendLine("{0} value;", this.cg.bindingManager.GetCSTypeFullName(eventInfo.EventHandlerType));
            var getter = this.cg.bindingManager.GetScriptObjectGetter(eventInfo.EventHandlerType, "ctx", "argv[0]", "value");
            this.cg.cs.AppendLine("{0};", getter);
            this.cg.cs.AppendLine("{0}.{1} -= value;", caller, eventInfo.Name);
            if (declaringType.IsValueType && !eventInfo.GetAddMethod().IsStatic)
            {
                // 非静态结构体属性修改, 尝试替换实例
                this.cg.cs.AppendLine($"js_rebind_this(ctx, this_obj, {caller});");
            }
            this.cg.cs.AppendLine("return JSApi.JS_UNDEFINED;");
        }

        public virtual void Dispose()
        {
        }
    }

    public class EventProxyCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected EventBindingInfo eventBindingInfo;

        public EventProxyCodeGen(CodeGenerator cg, EventBindingInfo eventBindingInfo)
        {
            this.cg = cg;
            this.eventBindingInfo = eventBindingInfo;

            // var eventInfo = this.eventBindingInfo.eventInfo;
            // var declaringType = eventInfo.DeclaringType;
            // var tsFieldVar = BindingManager.GetTSVariable(eventBindingInfo.regName);
            var caller = this.cg.AppendGetThisCS(eventBindingInfo);
            this.cg.cs.AppendLine("return js_new_event(ctx, this_obj, {0}, \"*{1}\", {2}, {3});", caller, this.eventBindingInfo.regName, this.eventBindingInfo.adderName, this.eventBindingInfo.removerName);
        }

        public virtual void Dispose()
        {
        }
    }
}
