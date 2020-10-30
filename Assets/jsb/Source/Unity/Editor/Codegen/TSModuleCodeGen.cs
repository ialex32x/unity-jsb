using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class TSModuleCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected TypeBindingInfo typeBindingInfo;

        /// <summary>
        /// 当前模块名
        /// </summary>
        protected string tsModule;
        protected TSModuleBindingInfo moduleBindingInfo;

        public class ModuleInfo
        {
            // entry-name => alias-name
            // 引用此模块中类型的命名映射表 
            public Dictionary<string, string> alias = new Dictionary<string, string>();
        }

        // module-name => module-info
        // 引用的模块列表
        protected Dictionary<string, ModuleInfo> _modules = new Dictionary<string, ModuleInfo>();

        // unique alias-name
        protected HashSet<string> _uniqueNames = new HashSet<string>();

        public TSModuleCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo)
        {
            this.cg = cg;
            this.typeBindingInfo = typeBindingInfo;
            this.tsModule = string.IsNullOrEmpty(typeBindingInfo.tsTypeNaming.jsModule) ? "global" : typeBindingInfo.tsTypeNaming.jsModule;
            this.moduleBindingInfo = cg.bindingManager.GetExportedModule(typeBindingInfo.tsTypeNaming.jsModule);

            this.cg.tsDeclare.AppendLine($"declare module \"{this.tsModule}\" {{");
            this.cg.tsDeclare.AddTabLevel();

            //TODO: generate 'import' statements
            AddModuleAlias(typeBindingInfo.super);
            foreach (var entry in typeBindingInfo.fields)
            {
                AddModuleAlias(entry.Value.fieldType);
            }

            foreach (var entry in typeBindingInfo.properties)
            {
                AddModuleAlias(entry.Value.propertyType);
            }

            foreach (var me in _modules)
            {
                var moduleName = me.Key;
                var moduleInfo = me.Value;
                var count = moduleInfo.alias.Count;

                if (count > 0)
                {
                    var index = 0;

                    this.cg.tsDeclare.Append($"import {{ ");
                    foreach (var pair in moduleInfo.alias)
                    {
                        var entry = pair.Key;
                        var alias = pair.Value;

                        if (entry != alias)
                        {
                            this.cg.tsDeclare.AppendL($"{entry} as {alias}");
                        }
                        else
                        {
                            this.cg.tsDeclare.AppendL($"{entry}");
                        }

                        if (index != count - 1)
                        {
                            this.cg.tsDeclare.AppendL(", ");
                        }
                        ++index;
                    }
                    this.cg.tsDeclare.AppendL($" }} from \"{moduleName}\";");
                    this.cg.tsDeclare.AppendLine();
                }
            }
        }

        private void AddModuleAlias(Type type)
        {
            if (type == null || type.IsPrimitive || type == typeof(string))
            {
                return;
            }

            var tsTypeNaming = cg.bindingManager.GetTSTypeNaming(type);

            // 避免引入自身
            if (tsTypeNaming != null && tsTypeNaming.jsModule != this.typeBindingInfo.tsTypeNaming.jsModule)
            {
                AddModuleAlias(tsTypeNaming.jsModule, tsTypeNaming.jsModuleAccess);
            }
        }

        private void AddModuleAlias(string moduleName, string accessName)
        {
            ModuleInfo reg;
            if (!_modules.TryGetValue(moduleName, out reg))
            {
                reg = _modules[moduleName] = new ModuleInfo();
            }

            if (!reg.alias.ContainsKey(accessName))
            {
                var uniqueName = GetUniqueAccess(accessName, 0);
                reg.alias.Add(accessName, uniqueName);
            }
        }

        // 如果是当前模块中的命名, 则拥有绝对优先权
        private string GetUniqueAccess(string uname, int index)
        {
            var rename = index == 0 ? uname : uname + index;

            if (this.moduleBindingInfo.ContainsKey(rename) || _uniqueNames.Contains(rename))
            {
                return GetUniqueAccess(uname, index + 1);
            }

            _uniqueNames.Add(rename);
            return rename;
        }

        public void Dispose()
        {
            this.cg.tsDeclare.DecTabLevel();
            this.cg.tsDeclare.AppendLine("}");
        }

        #region TS 命名辅助

        public string GetTSTypeFullName(Type type)
        {
            return GetTSTypeFullName(this.cg.bindingManager.GetExportedType(type));
        }

        public string GetTSTypeFullName(TypeBindingInfo typeBindingInfo)
        {
            if (typeBindingInfo == null)
            {
                return "";
            }

            if (typeBindingInfo.tsTypeNaming.jsModule == this.tsModule)
            {
                var s = this.cg.bindingManager.GetTSTypeFullName(typeBindingInfo);
                if (s.StartsWith(this.tsModule))
                {
                    return s.Substring(this.tsModule.Length + 1);
                }
                // Debug.Log($"{s} ?? {this.tsModule}");
                return s;
            }

            ModuleInfo moduleInfo;
            if (_modules.TryGetValue(typeBindingInfo.tsTypeNaming.jsModule, out moduleInfo))
            {
                string alias;
                if (moduleInfo.alias.TryGetValue(typeBindingInfo.tsTypeNaming.jsModuleAccess, out alias))
                {
                    return this.cg.bindingManager.GetTSTypeLocalName(typeBindingInfo, alias);
                }
            }

            return this.cg.bindingManager.GetTSTypeFullName(typeBindingInfo);
        }

        #endregion
    }
}