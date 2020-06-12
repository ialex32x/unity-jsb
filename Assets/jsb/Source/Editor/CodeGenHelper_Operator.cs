using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Editor
{
    // 生成成员方法绑定代码
    public class OperatorCodeGen : MethodBaseCodeGen<MethodInfo>
    {
        protected OperatorBindingInfo bindingInfo;

        protected override Type GetReturnType(MethodInfo method)
        {
            return method.ReturnType;
        }

        protected override string GetInvokeBinding(string caller, MethodInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
        {
            var arglist = Concat(AppendGetParameters(hasParams, nargs, parameters, parametersByRef), " " + bindingInfo.regName + " ");
            var transform = cg.bindingManager.GetTypeTransform(method.DeclaringType);
            if (transform == null || !transform.OnBinding(BindingPoints.METHOD_BINDING_BEFORE_INVOKE, method, cg))
            {
            }

            return arglist;
        }

        public OperatorCodeGen(CodeGenerator cg, OperatorBindingInfo bindingInfo)
            : base(cg)
        {
            this.bindingInfo = bindingInfo;
            WriteAllVariants(this.bindingInfo);
            // WriteTSAllVariants(this.bindingInfo);
        }
    }

    public class TSOperatorCodeGen : MethodBaseCodeGen<MethodInfo>
    {
        protected OperatorBindingInfo bindingInfo;

        protected override Type GetReturnType(MethodInfo method)
        {
            return method.ReturnType;
        }

        protected override string GetInvokeBinding(string caller, MethodInfo method, bool hasParams, bool isExtension, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
        {
            return null;
        }

        public TSOperatorCodeGen(CodeGenerator cg, OperatorBindingInfo bindingInfo)
            : base(cg)
        {
            this.bindingInfo = bindingInfo;
            WriteTSAllVariants(this.bindingInfo);
        }
    }
}