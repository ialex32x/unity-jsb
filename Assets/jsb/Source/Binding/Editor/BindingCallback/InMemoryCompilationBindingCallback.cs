using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace QuickJS.Binding
{
    public class InMemoryCompilationBindingCallback : IBindingCallback, ICodeGenCallback
    {
        private ScriptRuntime _runtime;
        private BindingManager _bindingManager;

        private string _namespace = Values.NamespaceOfStaticBinder;
        private string _className;
        private HashSet<Assembly> _referencedAssemblies = new HashSet<Assembly>();
        private List<Assembly> _generatedAssemblies = new List<Assembly>();
        private string _compilerOptions;

        public InMemoryCompilationBindingCallback(ScriptRuntime runtime)
        {
            _runtime = runtime;
            _className = "_GeneratedClass_" + Guid.NewGuid().ToString().Replace("-", "");
            var symbolList = new List<string>();
            var defines = "";
            var compilerOptions = "-unsafe";

#if !JSB_UNITYLESS
            symbolList.AddRange(Unity.UnityHelper.GetDefinedSymbols());
#endif

            defines += string.Join(";", symbolList);

#if !JSB_UNITYLESS && UNITY_EDITOR
            var customDefinedSymbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(Unity.UnityHelper.GetBuildTargetGroup());
            if (!string.IsNullOrEmpty(customDefinedSymbols))
            {
                defines += ";" + customDefinedSymbols;
            }
#endif
            if (!string.IsNullOrEmpty(defines))
            {
                compilerOptions += " -defines:" + defines;
            }
            _compilerOptions = compilerOptions;
        }

        public void OnBindingBegin(BindingManager bindingManager)
        {
            _bindingManager = bindingManager;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                {
                    _referencedAssemblies.Add(assembly);
                }
            }
        }

        public void OnBindingEnd()
        {
        }

        public void BeginStaticModule(string moduleName, int capacity)
        {
        }

        public void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo)
        {
        }

        public void EndStaticModule(string moduleName)
        {
        }

        public void AddDelegate(DelegateBridgeBindingInfo bindingInfo)
        {
        }

        public void OnCodeGenBegin(BindingManager bindingManager)
        {
        }

        public void OnCodeGenEnd()
        {
        }

        public bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total)
        {
            return false;
        }

        public void OnGenerateFinish()
        {
        }

        public void OnSourceCodeEmitted(CodeGenerator cg, string codeOutDir, string codeName, SourceCodeType type, string source)
        {
            if (type == SourceCodeType.CSharp)
            {
                if (codeName == CodeGenerator.NameOfBindingList)
                {
                    var list = new List<Assembly>(_generatedAssemblies);
                    list.AddRange(_referencedAssemblies);
                    var assembly = CompileSource(source, codeName, list);
                    var Class = assembly.GetType(_namespace + "." + _className);
                    var BindAll = Class?.GetMethod(Values.MethodNameOfStaticBinder);

                    BindAll.Invoke(null, new object[] { _runtime });
                }
                else
                {
                    _generatedAssemblies.Add(CompileSource(source, codeName, _referencedAssemblies));
                }
            }
        }

        public void OnGenerateBindingList(CodeGenerator cg, IEnumerable<IGrouping<string, TypeBindingInfo>> modules)
        {
            cg.GenerateBindingList(_namespace, _className, modules, false);
        }

        private Assembly CompileSource(string source, string assemblyName, IEnumerable<Assembly> referencedAssemblies)
        {
            using (var codeDomProvider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("cs"))
            {
                var compilerParameters = new System.CodeDom.Compiler.CompilerParameters();
                compilerParameters.GenerateInMemory = true;
                compilerParameters.TreatWarningsAsErrors = false;
                compilerParameters.CompilerOptions = _compilerOptions;
                compilerParameters.OutputAssembly = "_InMemory_" + assemblyName;
                compilerParameters.ReferencedAssemblies.AddRange((from a in referencedAssemblies select a.Location).ToArray());
                var result = codeDomProvider.CompileAssemblyFromSource(compilerParameters, source);

                if (result.Errors.HasErrors)
                {
                    _bindingManager.Error(string.Format("failed to compile source [{0} errors]", result.Errors.Count));
                    foreach (var err in result.Errors)
                    {
                        _bindingManager.Error(err.ToString());
                    }
                    throw new InvalidOperationException("failed to compile");
                }
                else
                {
                    return result.CompiledAssembly;
                }
            }
        }
    }
}
