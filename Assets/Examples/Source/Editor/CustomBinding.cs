using System.Reflection;

namespace Example.Editor
{
    using QuickJS.Unity;
    using UnityEngine;

    public class CustomBinding : AbstractBindingProcess
    {
        public override string GetBindingProcessName()
        {
            return "Example";
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            bindingManager.AddExportedType(typeof(WaitForSeconds), true);
            bindingManager.AddExportedType(typeof(WaitForEndOfFrame), true);
            bindingManager.AddExportedType(typeof(Time));
            bindingManager.AddExportedType(typeof(Input));
            bindingManager.AddExportedType(typeof(Ray));
            bindingManager.AddExportedType(typeof(Rect));
            bindingManager.AddExportedType(typeof(RaycastHit));
            bindingManager.AddExportedType(typeof(Physics)); // 无法自动处理部分重载
            bindingManager.AddExportedType(typeof(System.Net.IPHostEntry)).SystemRuntime();

            bindingManager.AddExportedType(typeof(System.Enum)).SystemRuntime();
            bindingManager.AddExportedType(typeof(System.IO.File)).SystemRuntime()
                .SetMemberBlocked("GetAccessControl")
                .SetMemberBlocked("SetAccessControl")
                .OnFilter<MethodInfo>(info => info.GetParameters().Length == 4); // not available in .net standard 2.0

            bindingManager.AddExportedType(typeof(TWrapper<int>));
            bindingManager.AddExportedType(typeof(TWrapper<Vector3>));
        }
    }
}