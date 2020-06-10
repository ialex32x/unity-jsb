using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class ScriptEngineStatsWindow : BaseEditorWindow
    {
        private Vector2 _sv;
        private bool _touch;
        private Native.JSMemoryUsage _memoryUsage;

        [MenuItem("JS Bridge/Stats Viewer")]
        static void OpenThis()
        {
            GetWindow<ScriptEngineStatsWindow>().Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("ScriptEngine Stats");
        }

        string ToSizeText(long size)
        {
            if (size > 1024 * 1024 * 2)
            {
                return string.Format("{0:.0} MB", (float)size / 1024f / 1024f);
            }
            else if (size > 1024 * 2)
            {
                return string.Format("{0:.0} KB", (float)size / 1024f);
            }
            return string.Format("{0} B", (int)size);
        }

        string ToCountText(long size)
        {
            return size.ToString();
        }

        void Capture(ScriptRuntime runtime)
        {
            _touch = true;
            unsafe
            {
                fixed (Native.JSMemoryUsage* ptr = &_memoryUsage)
                {
                    Native.JSApi.JS_ComputeMemoryUsage(runtime, ptr);
                }
            }
        }

        protected override void OnPaint()
        {
            var runtime = ScriptEngine.GetRuntime();

            if (runtime == null)
            {
                EditorGUILayout.HelpBox("No Running Runtime", MessageType.Info);
                return;
            }

            if (!_touch)
            {
                Capture(runtime);
            }

            if (GUILayout.Button("Capture"))
            {
                Capture(runtime);
            }

            _sv = EditorGUILayout.BeginScrollView(_sv);
            Block("JSMemoryUsage", () =>
            {
                EditorGUILayout.TextField("malloc_size", ToSizeText(_memoryUsage.malloc_size));
                EditorGUILayout.TextField("malloc_limit", ToCountText(_memoryUsage.malloc_limit));
                EditorGUILayout.TextField("memory_used_size", ToSizeText(_memoryUsage.memory_used_size));
                EditorGUILayout.TextField("malloc_count", ToCountText(_memoryUsage.malloc_count));
                EditorGUILayout.TextField("memory_used_count", ToCountText(_memoryUsage.memory_used_count));
                EditorGUILayout.TextField("atom_count", ToCountText(_memoryUsage.atom_count));
                EditorGUILayout.TextField("atom_size", ToSizeText(_memoryUsage.atom_size));
                EditorGUILayout.TextField("str_count", ToCountText(_memoryUsage.str_count));
                EditorGUILayout.TextField("str_size", ToSizeText(_memoryUsage.str_size));
                EditorGUILayout.TextField("obj_count", ToCountText(_memoryUsage.obj_count));
                EditorGUILayout.TextField("obj_size", ToSizeText(_memoryUsage.obj_size));
                EditorGUILayout.TextField("prop_count", ToCountText(_memoryUsage.prop_count));
                EditorGUILayout.TextField("prop_size", ToSizeText(_memoryUsage.prop_size));
                EditorGUILayout.TextField("shape_count", ToCountText(_memoryUsage.shape_count));
                EditorGUILayout.TextField("shape_size", ToSizeText(_memoryUsage.shape_size));
                EditorGUILayout.TextField("js_func_count", ToCountText(_memoryUsage.js_func_count));
                EditorGUILayout.TextField("js_func_size", ToSizeText(_memoryUsage.js_func_size));
                EditorGUILayout.TextField("js_func_code_size", ToSizeText(_memoryUsage.js_func_code_size));
                EditorGUILayout.TextField("js_func_pc2line_count", ToCountText(_memoryUsage.js_func_pc2line_count));
                EditorGUILayout.TextField("js_func_pc2line_size", ToSizeText(_memoryUsage.js_func_pc2line_size));
                EditorGUILayout.TextField("c_func_count", ToCountText(_memoryUsage.c_func_count));
                EditorGUILayout.TextField("array_count", ToCountText(_memoryUsage.array_count));
                EditorGUILayout.TextField("fast_array_count", ToCountText(_memoryUsage.fast_array_count));
                EditorGUILayout.TextField("fast_array_elements", ToCountText(_memoryUsage.fast_array_elements));
                EditorGUILayout.TextField("binary_object_count", ToCountText(_memoryUsage.binary_object_count));
                EditorGUILayout.TextField("binary_object_size", ToSizeText(_memoryUsage.binary_object_size));
            });

            Block("Misc.", () =>
            {
                var typeDB = runtime.GetTypeDB();
                EditorGUILayout.IntField("Exported Types", typeDB.Count);

                var objectCache = runtime.GetObjectCache();
                EditorGUILayout.IntField("ManagedObject Count", objectCache.GetManagedObjectCount());
                EditorGUILayout.IntField("JSObject Count", objectCache.GetJSObjectCount());
                EditorGUILayout.IntField("Delegate Count", objectCache.GetDelegateCount());

                var timeManager = runtime.GetTimerManager();
                EditorGUILayout.IntField("Active Timer", timeManager.GetActiveTimeHandleCount());
            });

            EditorGUILayout.EndScrollView();
        }
    }
}
