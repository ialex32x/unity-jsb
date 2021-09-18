using System.Reflection;
using System;

namespace Example.Editor
{
    using QuickJS.Unity;
    using QuickJS.Binding;
    using UnityEngine;

    public class CustomBinding : AbstractBindingProcess
    {
        public override string GetBindingProcessName()
        {
            return "Example";
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            // bindingManager.TryExportExtensionMethods(typeof(ExtensionTest));  // expose all extension methods
            bindingManager.AddExtensionMethod<Transform>(ExtensionTest.ResetAll); // expose single extension method

            bindingManager.AddExportedType(typeof(ParticleSystem));
            bindingManager.AddExportedType(typeof(ParticleSystemRenderer))
                .SetMemberBlocked("supportsMeshInstancing")
            ;
            bindingManager.AddExportedType(typeof(ParticleSystem.MainModule));
            bindingManager.AddExportedType(typeof(ParticleSystemSimulationSpace));
            bindingManager.AddExportedType(typeof(System.Net.IPHostEntry)).SystemRuntime();

            bindingManager.AddExportedType(typeof(System.DateTime)).SystemRuntime().EnableOperatorOverloading(false);
            bindingManager.AddExportedType(typeof(System.IO.FileInfo)).SystemRuntime()
                .SetMemberBlocked("GetAccessControl")
                .SetMemberBlocked("SetAccessControl");
            bindingManager.AddExportedType(typeof(System.IO.File)).SystemRuntime()
                .SetMemberBlocked("GetAccessControl")
                .SetMemberBlocked("SetAccessControl")
                .OnFilter<MethodInfo>(info => info.GetParameters().Length == 4); // not available in .net standard 2.0

            bindingManager.AddExportedType(typeof(TWrapper<int>));
            bindingManager.AddExportedType(typeof(TWrapper<Vector3>));
            bindingManager.AddExportedType(typeof(DisposableObject)).SetDisposable();

#if CUSTOM_DEF_FOO && UNITY_EDITOR
            bindingManager.AddExportedType(typeof(FOO)).AddRequiredDefines("CUSTOM_DEF_FOO", "UNITY_EDITOR")
#if CUSTOM_DEF_PROP
                .AddRequiredDefinesForMember("propValue", "CUSTOM_DEF_PROP")                
#endif
#if CUSTOM_DEF_METHOD
                .AddRequiredDefinesForMethod(t => t.GetMethod("Exclusive", Type.EmptyTypes), "CUSTOM_DEF_METHOD")
#endif
            ;
#endif
#if CUSTOM_DEF_BAR
            bindingManager.AddExportedType(typeof(BAR)).AddRequiredDefines("CUSTOM_DEF_BAR");
#endif
        }
    }
}
