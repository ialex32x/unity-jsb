using QuickJS.Editor;
using UnityEngine;

namespace jsb.Editor
{
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

#if UNITY_EDITOR 
            // [test] editor only
            bindingManager.AddExportedType(typeof(UnityEditor.EditorApplication), false, true);
#endif
        }
    }
}