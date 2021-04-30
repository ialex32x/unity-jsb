using System.Reflection;

namespace Example.Editor
{
    using QuickJS.Binding;

    public class CustomBinding : AbstractBindingProcess
    {
        public override string GetBindingProcessName()
        {
            return "dotnetcore";
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            bindingManager.AddExportedType(typeof(System.Math));
        }
        
        public override void OnPostExporting(BindingManager bindingManager)
        {
            // bindingManager.ExportTypesInAssembly(typeof(System.Console).Assembly, true);
        }
    }
}