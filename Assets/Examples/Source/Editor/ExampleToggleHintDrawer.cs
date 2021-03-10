using System;
using System.IO;
using System.Linq;

namespace Example.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer((typeof(ExampleToggleHintAttribute)))]
    public class ExampleToggleHintDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            var ta = attribute as ExampleToggleHintAttribute;
            property.boolValue = EditorGUI.Toggle(pos, ta.text, property.boolValue);
        }
    }
}