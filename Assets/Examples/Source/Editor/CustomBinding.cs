using QuickJS.Editor;
using System.Reflection;

namespace jsb.Editor
{
    using UnityEngine;
    public class CustomBinding : AbstractBindingProcess
    {
        public override void OnPreExporting(BindingManager bindingManager)
        {
            bindingManager.AddExportedType(typeof(WaitForSeconds), true);
            bindingManager.AddExportedType(typeof(WaitForEndOfFrame), true);
            // AddExportedType(typeof(System.Collections.IEnumerator), true);
            bindingManager.AddExportedType(typeof(Time));
            // bindingManager.AddExportedType(typeof(System.Net.Dns));
            bindingManager.AddExportedType(typeof(System.Net.IPHostEntry));

            bindingManager.AddExportedType(typeof(System.Enum));
            bindingManager.AddExportedType(typeof(System.IO.File))
                .SetMemberBlocked("GetAccessControl")
                .SetMemberBlocked("SetAccessControl")
                .OnFilter<MethodInfo>(info => info.GetParameters().Length == 4); // not available in .net standard 2.0

#if UNITY_EDITOR 
            // [test] editor only
            bindingManager.AddExportedType(typeof(UnityEditor.EditorApplication), false, true);
            bindingManager.AddExportedType(typeof(UnityEditor.EditorWindow), false, true);
#endif
        }
    }
}