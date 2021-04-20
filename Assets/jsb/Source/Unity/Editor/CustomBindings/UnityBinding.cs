using System;
using System.Reflection;

namespace jsb.Editor
{
    using QuickJS.Native;
    using QuickJS.Unity;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    public class UnityBinding : AbstractBindingProcess
    {
        public override string GetBindingProcessName()
        {
            return "Unity (Basic)";
        }

        public bool IsAvailable(MethodInfo methodInfo)
        {
            return methodInfo != null && methodInfo.IsPublic;
        }

        public override void OnPreCollectAssemblies(BindingManager bindingManager)
        {
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            bindingManager.AddExportedType(typeof(Resources)).SetAllConstructorsBlocked();
        }
    }
}
