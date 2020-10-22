using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class EnumCodeGen : TypeCodeGen
    {
        public EnumCodeGen(CodeGenerator cg, TypeBindingInfo type)
        : base(cg, type)
        {
            this.cg.AppendJSDoc(type.type);
            var prefix = this.typeBindingInfo.topLevel ? "declare " : "";
            this.cg.tsDeclare.AppendLine("{0}enum {1} {{", prefix, typeBindingInfo.jsName);
            this.cg.tsDeclare.AddTabLevel();
        }

        public override void Dispose()
        {
            using (new RegFuncCodeGen(cg))
            {
                using (new RegFuncNamespaceCodeGen(cg, typeBindingInfo))
                {
                    this.cg.cs.AppendLine("var cls = ns.CreateEnum(\"{0}\", typeof({1}));",
                        typeBindingInfo.jsName,
                        this.cg.bindingManager.GetCSTypeFullName(typeBindingInfo.type));
                    var values = new Dictionary<string, object>();
                    foreach (var ev in Enum.GetValues(typeBindingInfo.type))
                    {
                        values[Enum.GetName(typeBindingInfo.type, ev)] = ev;
                    }
                    foreach (var kv in values)
                    {
                        var name = kv.Key;
                        var value = kv.Value;
                        var pvalue = Convert.ToInt32(value);
                        this.cg.cs.AppendLine($"cls.AddConstValue(\"{name}\", {pvalue});");
                        this.cg.AppendEnumJSDoc(typeBindingInfo.type, value);
                        this.cg.tsDeclare.AppendLine($"{name} = {pvalue},");
                    }
                    this.cg.cs.AppendLine("cls.Close();");
                }
            }
            base.Dispose();
            this.cg.tsDeclare.DecTabLevel();
            this.cg.tsDeclare.AppendLine("}");
        }
    }
}
