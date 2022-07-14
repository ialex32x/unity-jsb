#if UNITY_EDITOR || JSB_RUNTIME_REFLECT_BINDING
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public class TSModuleCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected TypeBindingInfo typeBindingInfo;

        protected static HashSet<Type> _noImportTypes = new HashSet<Type>(new Type[]
        {
            typeof(void),
            typeof(string),
            typeof(Delegate),
        });

        /// <summary>
        /// current module name
        /// </summary>
        protected readonly string moduleName;

        protected TSModuleBindingInfo moduleBindingInfo;

        public class ModuleInfo
        {
            /// <summary>
            /// a name map for all referenced types from other modules <br/>
            /// entry-name => alias-name 
            /// </summary>
            public Dictionary<string, string> alias = new Dictionary<string, string>();
        }


        /// <summary>
        /// all referenced modules <br/>
        /// module-name => module-info
        /// </summary>
        protected Dictionary<string, ModuleInfo> _modules = new Dictionary<string, ModuleInfo>();

        /// <summary>
        /// records of unique local names for all referenced types from other modules
        /// </summary>
        protected HashSet<string> _uniqueNames = new HashSet<string>();

        public TSModuleCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo)
        {
            this.cg = cg;
            this.typeBindingInfo = typeBindingInfo;
            this.moduleName = typeBindingInfo.tsTypeNaming.moduleName;
            this.moduleBindingInfo = cg.bindingManager.GetExportedTSModule(this.moduleName);

            this.cg.tsDeclare.BeginPart();
            this.cg.tsDeclare.AppendLine($"declare module \"{GetModuleName(this.moduleName)}\" {{");
            this.cg.tsDeclare.AddTabLevel();

            CollectImports();
            WriteImports();
        }

        private string GetModuleName(string rawModuleName)
        {
            return string.IsNullOrEmpty(rawModuleName) ? cg.bindingManager.prefs.defaultJSModule : rawModuleName;
        }

        public void Dispose()
        {
            this.cg.tsDeclare.DecTabLevel();
            this.cg.tsDeclare.AppendLine("}");
            this.cg.tsDeclare.EndPart();
        }

        private void CollectImports()
        {
            if (typeBindingInfo.super != typeof(Enum))
            {
                AddModuleAlias(typeBindingInfo.super);
            }

            foreach (var @interface in typeBindingInfo.interfaces)
            {
                AddModuleAlias(@interface);
            }

            foreach (var entry in typeBindingInfo.fields)
            {
                AddModuleAlias(entry.Value.fieldType);
            }

            foreach (var entry in typeBindingInfo.properties)
            {
                AddModuleAlias(entry.Value.propertyType);
            }

            foreach (var entry in typeBindingInfo.events)
            {
                AddModuleAlias(entry.Value.eventInfo.EventHandlerType);
            }

            // process all type references in delegates
            foreach (var @delegate in typeBindingInfo.delegates)
            {
                AddModuleAlias(@delegate.Value.delegateType);
            }

            // process all type references in constructors
            foreach (var entryVariant in typeBindingInfo.constructors.variants)
            {
                foreach (var method in entryVariant.Value.plainMethods)
                {
                    foreach (var p in method.method.GetParameters())
                    {
                        AddModuleAlias(p);
                    }
                }

                foreach (var method in entryVariant.Value.varargMethods)
                {
                    foreach (var p in method.method.GetParameters())
                    {
                        AddModuleAlias(p);
                    }
                }
            }

            // process all type references in methods
            var methods = typeBindingInfo.staticMethods.Select(s => s.Value)
                .Concat(typeBindingInfo.methods.Select(s => s.Value));

            foreach (var entry in methods)
            {
                foreach (var entryVariant in entry.variants)
                {
                    foreach (var method in entryVariant.Value.plainMethods)
                    {
                        AddModuleAlias(method.method.ReturnType);
                        foreach (var p in method.method.GetParameters())
                        {
                            AddModuleAlias(p);
                        }
                    }

                    foreach (var method in entryVariant.Value.varargMethods)
                    {
                        AddModuleAlias(method.method.ReturnType);
                        foreach (var p in method.method.GetParameters())
                        {
                            AddModuleAlias(p);
                        }
                    }
                }
            }
        }

        private void WriteImports()
        {
            // always import jsb for simplicity (ref, out, Nullable<>, byte)
            this.cg.tsDeclare.AppendLine($"import * as jsb from \"jsb\";");

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
                    this.cg.tsDeclare.AppendL($" }} from \"{GetModuleName(moduleName)}\";");
                    this.cg.tsDeclare.AppendLine();
                }
            }
        }

        private void AddModuleAlias(ParameterInfo p)
        {
            AddModuleAlias(p.IsDefined(typeof(ParamArrayAttribute)) ? p.ParameterType.GetElementType() : p.ParameterType);
        }

        //TODO hardcoded, refactor it later
        private string GetCSharpArray()
        {
            var prefs = typeBindingInfo.bindingManager.prefs;
            if (prefs.GetModuleStyle() == ETSModuleStyle.Singular)
            {
                return "System.CSharpArray";
            }
            return "CSharpArray";
        }

        private void AddBuiltinModuleAlias(string topLevel, string typeName)
        {
            var prefs = typeBindingInfo.bindingManager.prefs;
            if (prefs.GetModuleStyle() == ETSModuleStyle.Singular)
            {
                AddModuleAlias(prefs.singularModuleName, topLevel);
            }
            else
            {
                AddModuleAlias(topLevel, typeName);
            }
        }

        private void AddModuleAlias(Type originalType)
        {
            if (originalType == null || originalType.IsPrimitive || _noImportTypes.Contains(originalType))
            {
                return;
            }

            if (originalType == typeof(Enum))
            {
                AddBuiltinModuleAlias("System", "Enum");
                return;
            }

            if (originalType.IsArray)
            {
                AddBuiltinModuleAlias("System", "CSharpArray");
                AddModuleAlias(originalType.GetElementType());
                return;
            }

            if (originalType.IsByRef)
            {
                AddModuleAlias(originalType.GetElementType());
                return;
            }

            if (originalType.IsGenericType && !originalType.IsGenericTypeDefinition)
            {
                foreach (var g in originalType.GetGenericArguments())
                {
                    AddModuleAlias(g);
                }

                AddModuleAlias(originalType.GetGenericTypeDefinition());
                return;
            }

            var tsTypeNaming = cg.bindingManager.GetTSTypeNaming(originalType);
            if (tsTypeNaming != null)
            {
                // 避免引入自身
                if (tsTypeNaming.moduleName != this.typeBindingInfo.tsTypeNaming.moduleName)
                {
                    AddModuleAlias(tsTypeNaming.moduleName, tsTypeNaming.moduleEntry);
                }
            }
            else
            {
                var delegateBindingInfo = this.cg.bindingManager.GetDelegateBindingInfo(originalType);

                if (delegateBindingInfo != null)
                {
                    AddModuleAlias(delegateBindingInfo.returnType);
                    foreach (var p in delegateBindingInfo.parameters)
                    {
                        AddModuleAlias(p.ParameterType);
                    }
                }
                else
                {
                    var exported = this.cg.bindingManager.GetExportedTypeRecursively(originalType);
                    if (exported != null && exported.type != originalType)
                    {
                        AddModuleAlias(exported.type);
                    }
                }
            }
        }

        private void AddModuleAlias(string moduleName, string moduleEntry)
        {
            // 手工添加的模块访问需要过滤掉本模块自身 
            // 例如: AddModuleAlias("System", "Array")
            if (moduleName == this.moduleName)
            {
                return;
            }

            ModuleInfo reg;
            if (!_modules.TryGetValue(moduleName, out reg))
            {
                reg = _modules[moduleName] = new ModuleInfo();
            }

            if (!reg.alias.ContainsKey(moduleEntry))
            {
                var uniqueName = GetUniqueAccess(moduleEntry);
                reg.alias.Add(moduleEntry, uniqueName);
            }
        }

        /// <summary>
        /// Generate a unique name for referencing in the current module. 
        /// The given name will be renamed with a number suffix if a type with the same name exists or it's already been generated before.
        /// </summary>
        private string GetUniqueAccess(string uname, int index = 0)
        {
            var rename = index == 0 ? uname : uname + index;

            if (this.moduleBindingInfo.Contains(rename) || _uniqueNames.Contains(rename))
            {
                return GetUniqueAccess(uname, index + 1);
            }

            _uniqueNames.Add(rename);
            return rename;
        }

        public string GetAlias(ITSTypeNaming tsTypeNaming)
        {
            // var tsTypeNaming = this.cg.bindingManager.GetTSTypeNaming(type);
            ModuleInfo moduleInfo;
            if (_modules.TryGetValue(tsTypeNaming.moduleName, out moduleInfo))
            {
                string alias;
                if (moduleInfo.alias.TryGetValue(tsTypeNaming.moduleEntry, out alias))
                {
                    return alias;
                }
            }

            return null;
        }

        // 获取 type 在 typescript 中对应类型名
        public string GetTSTypeFullName(Type type)
        {
            return GetTSTypeFullName(type, false);
        }

        public string GetTSTypeFullName(ParameterInfo p)
        {
            return GetTSTypeFullName(p.ParameterType, p.IsOut);
        }

        public string GetTSTypeFullName(Type type, bool isOut)
        {
            if (type == null || type == typeof(void))
            {
                return "void";
            }

            if (type.IsByRef)
            {
                if (isOut)
                {
                    return $"{this.cg.bindingManager.GetDefaultTypePrefix()}Out<{GetTSTypeFullName(type.GetElementType())}>";
                }
                return $"{this.cg.bindingManager.GetDefaultTypePrefix()}Ref<{GetTSTypeFullName(type.GetElementType())}>";
                // return GetTSTypeFullName(type.GetElementType());
            }

            List<string> names;
            if (this.cg.bindingManager.GetTSTypeNameMap(type, out names))
            {
                return names.Count > 1 ? $"({String.Join(" | ", names)})" : names[0];
            }

            if (type == typeof(Array))
            {
                return GetCSharpArray() + "<any>";
            }

            if (type == typeof(ScriptPromise))
            {
                return "Promise<void>";
            }

            if (type.IsSubclassOf(typeof(ScriptPromise)))
            {
                if (type.IsGenericType)
                {
                    var gt = type.GetGenericArguments()[0];
                    return "Promise<" + GetTSTypeFullName(gt) + ">";
                }
                return "Promise<any>";
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var tsFullName = GetTSTypeFullName(elementType);
                var rank = type.GetArrayRank();

                if (rank == 1)
                {
                    return GetCSharpArray() + "<" + tsFullName + ">";
                }
                return GetCSharpArray() + "<" + tsFullName + ", " + rank + ">";
            }

            var info = this.cg.bindingManager.GetExportedType(type);
            if (info != null)
            {
                var tsTypeNaming = info.tsTypeNaming;
                var localAlias = GetAlias(tsTypeNaming);

                if (type.IsGenericType)
                {
                    if (type.IsGenericTypeDefinition)
                    {
                        var joinedArgs = string.Join(", ", from arg in type.GetGenericArguments() select arg.Name);
                        var typeName = tsTypeNaming.GetFullName(localAlias);
                        return $"{typeName}<{joinedArgs}>";
                    }
                    else
                    {
                        var gType = type.GetGenericTypeDefinition();
                        var gTypeInfo = this.cg.bindingManager.GetExportedType(gType);
                        if (gTypeInfo != null)
                        {
                            var tArgs = type.GetGenericArguments();
                            var tArgCount = tArgs.Length;
                            var templateArgs = new string[tArgCount];
                            var typeName = gTypeInfo.tsTypeNaming.GetFullName(localAlias);

                            for (int i = 0; i < tArgCount; i++)
                            {
                                templateArgs[i] = GetTSTypeFullName(tArgs[i]);
                            }

                            var joinedArgs = CodeGenUtils.Join(", ", templateArgs);
                            return $"{typeName}<{joinedArgs}>";
                        }
                    }
                }

                return tsTypeNaming.GetFullName(localAlias);
            }

            if (type.BaseType == typeof(MulticastDelegate))
            {
                var delegateBindingInfo = this.cg.bindingManager.GetDelegateBindingInfo(type);
                if (delegateBindingInfo != null)
                {
                    var ret = GetTSTypeFullName(delegateBindingInfo.returnType);
                    var v_arglist = GetTSArglistTypes(delegateBindingInfo.parameters);
                    return $"({v_arglist}) => {ret}";
                }
            }

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var gArgs = type.GetGenericArguments();
                    var gArgsTS = GetTSTypeFullName(gArgs[0]);
                    return $"{this.cg.bindingManager.GetDefaultTypePrefix()}Nullable<{gArgsTS}>";
                }
            }
            else
            {
                if (type.IsGenericParameter)
                {
                    return type.Name;
                }
            }

            return "any";
        }

        // 生成参数对应的字符串形式参数列表定义 (typescript)
        public string GetTSArglistTypes(ParameterInfo[] parameters)
        {
            var size = parameters.Length;
            var arglist = "";
            if (size == 0)
            {
                return arglist;
            }
            for (var i = 0; i < size; i++)
            {
                var parameter = parameters[i];
                var typename = GetTSTypeFullName(parameter);
                arglist += this.cg.bindingManager.GetTSVariable(parameter) + ": ";
                arglist += typename;
                if (i != size - 1)
                {
                    arglist += ", ";
                }
            }
            return arglist;
        }

        // 获取实现的接口的ts声明
        public string GetTSInterfaceNames(Type type)
        {
            var interfaces = type.GetInterfaces();
            var str = "";

            foreach (var @interface in interfaces)
            {
                var interfaceBindingInfo = this.cg.bindingManager.GetExportedType(@interface);
                if (interfaceBindingInfo != null)
                {
                    // Debug.Log($"{type.Name} implements {@interface.Name}");
                    str = CodeGenUtils.Join(", ", str, GetTSTypeFullName(interfaceBindingInfo.type));
                }
            }

            return str;
        }
    }
}
#endif
