#if UNITY_EDITOR
using System;
using System.Reflection;

namespace QuickJS.Unity
{
    using QuickJS.Unity;
    using UnityEngine;
    using UnityEditor;

    public class UnityEditorBindingStub
    {
        public static EditorWindow CreateWindow(EditorWindow editorWindow, Type t)
        {
            //TODO: Not Implemented
            return default(EditorWindow);
        }
    }
}
#endif
