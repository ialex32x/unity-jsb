using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace QuickJS.Hotfix
{
    using UnityEditor;
    using UnityEngine;

    //TODO: 热更功能临时代码
    public class HotfixUtil
    {
        private const string TypeNameForInjectFlag = "_jsb_injected_flag_";

        [MenuItem("JS Bridge/Hotfix")]
        public static void RunHotfix()
        {
            Run();
        }

        private static bool IsHotfixTarget(TypeDefinition td)
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

        public static bool Collect(AssemblyDefinition a, List<TypeDefinition> delegateTypes)
        {
            foreach (var type in a.MainModule.Types)
            {
                if (type.Name == TypeNameForInjectFlag)
                {
                    return false;
                }
                if (type.FullName == "jsb._QuickJSDelegates")
                {
                    foreach (var nested in type.NestedTypes)
                    {
                        if (nested.BaseType.FullName == "System.MulticastDelegate")
                        {
                            delegateTypes.Add(nested);
                            // Debug.LogFormat("Nest: {0} based {1} in {2}", nested.FullName, nested.BaseType.FullName, type.FullName);
                        }
                    }
                }
            }
            return true;
        }

        public static bool IsParameterMatched(ParameterDefinition p1, ParameterDefinition p2)
        {
            return p1.ParameterType == p2.ParameterType && p1.IsOut == p2.IsOut;
        }

        public static bool IsDelegateMatched(MethodDefinition m, TypeDefinition d)
        {
            var invoke = d.Methods.First(dm => dm.Name == "Invoke");
            var argc = invoke.Parameters.Count;
            if (argc != m.Parameters.Count + 1)
            {
                return false;
            }

            if (invoke.ReturnType != m.ReturnType)
            {
                return false;
            }

            if (invoke.Parameters[0].IsOut || invoke.Parameters[0].ParameterType != m.DeclaringType)
            {
                return false;
            }

            for (var i = 1; i < argc; i++)
            {
                var p1 = invoke.Parameters[i];
                var p2 = m.Parameters[i - 1];

                if (!IsParameterMatched(p1, p2))
                {
                    return false;
                }
            }

            return true;
        }

        public static TypeDefinition GetDelegate(MethodDefinition m, List<TypeDefinition> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (IsDelegateMatched(m, item))
                {
                    return item;
                }
            }

            return null;
        }

        public static string GetMethodString(MethodDefinition method)
        {
            var sb = "";
            sb += $"{method.ReturnType} ";
            sb += $"{method.Name}(";
            for (var i = 0; i < method.Parameters.Count; i++)
            {
                var p = method.Parameters[i];
                sb += $"{p.ParameterType} {p.Name}";
                if (i != method.Parameters.Count - 1)
                {
                    sb += ", ";
                }
            }
            sb += ");";

            return sb;
        }

        private static OpCode[] ldarg_i = new OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

        public static void Run()
        {
            var test = typeof(HotfixTest);
            var assemblyFilePath = test.Assembly.Location;
            var a = AssemblyDefinition.ReadAssembly(assemblyFilePath);
            var delegateTypes = new List<TypeDefinition>();

            if (!Collect(a, delegateTypes))
            {
                Debug.LogError("dirty dll");
                return;
            }

            a.MainModule.Types.Add(new TypeDefinition("QuickJS", TypeNameForInjectFlag, Mono.Cecil.TypeAttributes.Class, a.MainModule.TypeSystem.Object));
            foreach (var type in a.MainModule.Types)
            {
                if (!IsHotfixTarget(type))
                {
                    continue;
                }

                var sb = $"{type.FullName}\n";
                foreach (var method in type.Methods)
                {
                    var delegateType = GetDelegate(method, delegateTypes);
                    if (delegateType != null)
                    {
                        sb += GetMethodString(method) + " => " + delegateType.FullName;
                        var newLocal = new VariableDefinition(delegateType);
                        var newField = new FieldDefinition("_JSFIX_R_" + method.Name, FieldAttributes.Public | FieldAttributes.Static, delegateType);

                        var point = method.Body.Instructions[0];
                        var proc = method.Body.GetILProcessor();

                        type.Fields.Add(newField);
                        method.Body.Variables.Add(newLocal);

                        proc.InsertBefore(point, proc.Create(OpCodes.Ldsfld, newField));
                        proc.InsertBefore(point, proc.Create(OpCodes.Ldnull));
                        proc.InsertBefore(point, proc.Create(OpCodes.Cgt_Un));
                        proc.InsertBefore(point, proc.Create(OpCodes.Stloc, newLocal));
                        proc.InsertBefore(point, proc.Create(OpCodes.Ldloc, newLocal));
                        proc.InsertBefore(point, proc.Create(OpCodes.Brfalse_S, point)); // jump to original instructions
                        proc.InsertBefore(point, proc.Create(OpCodes.Ldsfld, newField));

                        var argCount = method.Parameters.Count;
                        if (!method.IsStatic)
                        {
                            argCount++;
                        }

                        for (var argIndex = 0; argIndex < argCount; argIndex++)
                        {
                            if (argIndex < ldarg_i.Length)
                            {
                                proc.InsertBefore(point, proc.Create(ldarg_i[argIndex]));
                            }
                            else if (argIndex <= byte.MaxValue)
                            {
                                proc.InsertBefore(point, proc.Create(OpCodes.Ldarg_S, (byte)argIndex));
                            }
                            else
                            {
                                proc.InsertBefore(point, proc.Create(OpCodes.Ldarg, argIndex));
                            }
                        }
                        var invoke = delegateType.Methods.First(dm => dm.Name == "Invoke");
                        proc.InsertBefore(point, proc.Create(OpCodes.Callvirt, invoke));
                        proc.InsertBefore(point, proc.Create(OpCodes.Ret));
                    }
                }
                Debug.LogFormat("{0}", sb);
            }

            a.Write(assemblyFilePath);
            // a.Write("temp.dll");
            Debug.LogFormat("write: {0}", assemblyFilePath);
        }
    }
}