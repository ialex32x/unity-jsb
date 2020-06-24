using System;
using System.Reflection;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public class DynamicType
    {
        // private Type _type;
        // private bool _privateAccess;

        // private IDynamicMethod _constructor;
        // private Dictionary<string, IDynamicMethod> _methods = new Dictionary<string, IDynamicMethod>();

        public DynamicType(Type type, bool privateAccess)
        {
            // _type = type;
            // _privateAccess = privateAccess;
        }

        public static void Bind(TypeRegister register, Type _type, bool _privateAccess)
        {
            // UnityEngine.Debug.LogErrorFormat("dynamic bind {0}", _type);
            var db = register.GetTypeDB();
            var ctx = (JSContext)register.GetContext();
            var flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
            var pflags = _privateAccess ? flags | BindingFlags.NonPublic : flags;
            var type_id = register.RegisterType(_type);

            #region BindConstructors(register, flags, type_id);
            var constructors = _type.GetConstructors(flags);
            var dynamicConstructor = default(IDynamicMethod);
            if (constructors.Length > 0)
            {
                var count = constructors.Length;
                if (count == 1)
                {
                    dynamicConstructor = new DynamicConstructor(constructors[0], type_id);
                }
                else
                {
                    var overloads = new DynamicMethods(count);
                    for (var i = 0; i < count; i++)
                    {
                        var overload = new DynamicConstructor(constructors[i], type_id);
                        overloads.Add(overload);
                    }
                    dynamicConstructor = overloads;
                }
            }
            #endregion 

            var cls = register.CreateClass(_type.Name, _type, dynamicConstructor);

            #region BindMethods(register, pflags);
            // var methods = _type.GetMethods(pflags);

            #endregion

            #region BindFields(register, pflags);
            var fieldInfos = _type.GetFields(pflags);
            for (int i = 0, count = fieldInfos.Length; i < count; i++)
            {
                var fieldInfo = fieldInfos[i];
                var dynamicField = new DynamicField(fieldInfo);
                cls.AddField(fieldInfo.IsStatic, fieldInfo.Name, dynamicField);
            }
            #endregion

            #region BindProperties(register, pflags);
            #endregion

            // var ns = new NamespaceDecl();
            // typeDB.AddType()
            cls.Close();
        }
    }
}
