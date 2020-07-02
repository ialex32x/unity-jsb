using System;

namespace QuickJS.Hotfix
{
    using UnityEditor;
    using UnityEngine;

    public class HotfixUtil
    {
        [MenuItem("JS Bridge/Hotfix")]
        public static void RunHotfix()
        {
            Run();
        }

        private static bool IsHotfixTarget(Mono.Cecil.TypeDefinition td)
        {
            foreach (var attr in td.CustomAttributes)
            {
                if (attr.AttributeType.FullName == typeof(QuickJS.JSHotfixAttribute).FullName)
                {
                    return true;
                }
            }
            return false;
        }

        public static void Run()
        {
            var test = typeof(HotfixTest);
            var a = Mono.Cecil.AssemblyDefinition.ReadAssembly(test.Assembly.Location);
            foreach (var type in a.MainModule.Types)
            {
                if (IsHotfixTarget(type))
                {
                    Debug.LogFormat("{0}", type.FullName);
                }
            }
        }
    }
}