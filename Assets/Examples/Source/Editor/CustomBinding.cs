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
            bindingManager.AddExportedType(typeof(Time));
        }
    }
}