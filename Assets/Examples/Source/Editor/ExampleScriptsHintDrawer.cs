using System;
using System.IO;
using System.Linq;

namespace Example.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer((typeof(ExampleScriptsHintAttribute)))]
    public class ExampleScriptsHintDrawer : PropertyDrawer
    {
        private GUIContent _default = new GUIContent("example_none");
        private GUIContent[] _options = null;

        private void RefreshOptions()
        {
            var ta = attribute as ExampleScriptsHintAttribute;
            
            _options = Directory.GetFiles(ta.path).Where(file => (file.Contains("example_") || file.Contains("game_")) && !file.EndsWith(".meta") && !file.EndsWith(".map"))
                .Select((file, i) => new GUIContent(new FileInfo(file).Name.Replace(".js", "")))
                .ToArray();

            if (_options.Length == 0)
            {
                ArrayUtility.Add(ref _options, _default);
            }
        }

        private int IndexOf(string v)
        {
            RefreshOptions();
            for (int i = 0, count = _options.Length; i < count; i++)
            {
                if (_options[i].text == v)
                {
                    return i;
                }
            }
            return -1;
        }

        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode);
            var index = IndexOf(property.stringValue);
            index = EditorGUI.Popup(new Rect(pos.x, pos.y, pos.width - 22f, pos.height), label, index, _options);
            if (GUI.Button(new Rect(pos.x + pos.width - 20f, pos.y, 20f, pos.height), "R"))
            {
                RefreshOptions();
            }
            property.stringValue = (index >= 0 ? _options[index] : _options[0]).text;
            EditorGUI.EndDisabledGroup();
        }
    }
}