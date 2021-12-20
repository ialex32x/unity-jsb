#if !JSB_UNITYLESS
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class ScriptEngineStatsWindow : BaseEditorWindow
    {
        private class Snapshot
        {
            public struct TimerInfo
            {
                public ulong id;
                public int delay;
                public int deadline;
                public bool once;
            }
            public int id;
            public bool alive;

            public Native.JSMemoryUsage memoryUsage;
            public bool isStaticBinding;
            public int exportedTypes;
            public int managedObjectCount;
            public int managedObjectCap;
            public bool fetchManagedObjectRefs;
            public List<WeakReference<object>> managedObjectRefs = new List<WeakReference<object>>();
            public int jSObjectCount;
            public int delegateCount;
            public int scriptValueCount;
            public int scriptPromiseCount;
            public int stringCount;
            public int timeNow;
            public List<TimerInfo> activeTimers = new List<TimerInfo>();
        }

        private Vector2 _sv;
        private int _alive;
        private bool _fetchManagedObjectRefs = false;
        private bool _autoCap = true;
        private float _time;
        private float _timeCap = 5f;
        private List<Snapshot> _snapshots = new List<Snapshot>();

        [MenuItem("JS Bridge/Stats Viewer", false, 5000)]
        static void OpenThis()
        {
            GetWindow<ScriptEngineStatsWindow>().Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("ScriptEngine Stats");
            _time = Time.realtimeSinceStartup;
            CaptureAll();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
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

        private Snapshot GetSnapshot(int id)
        {
            for (int i = 0, count = _snapshots.Count; i < count; i++)
            {
                var snapshot = _snapshots[i];
                if (snapshot.id == id)
                {
                    return snapshot;
                }
            }
            var inst = new Snapshot();
            inst.id = id;
            _snapshots.Add(inst);
            return inst;
        }

        private void Capture(ScriptRuntime runtime)
        {
            if (!runtime.isValid)
            {
                return;
            }

            var snapshot = GetSnapshot(runtime.id);
            snapshot.alive = true;
            snapshot.fetchManagedObjectRefs = _fetchManagedObjectRefs;
            runtime.EnqueueAction(OnSnapshotRequest, snapshot);
        }

        private static void OnSnapshotRequest(ScriptRuntime runtime, Utils.JSAction act)
        {
            if (!runtime.isValid || !runtime.isRunning)
            {
                // Debug.LogError("get snapshot on released script runtime");
                return;
            }
            var snapshot = (Snapshot)act.args;
            lock (snapshot)
            {
                unsafe
                {
                    fixed (Native.JSMemoryUsage* ptr = &snapshot.memoryUsage)
                    {
                        Native.JSApi.JS_ComputeMemoryUsage(runtime, ptr);
                    }
                }

                var typeDB = runtime.GetTypeDB();
                snapshot.exportedTypes = typeDB.Count;
                snapshot.isStaticBinding = runtime.isStaticBinding;

                var objectCache = runtime.GetObjectCache();
                var stringCache = runtime.GetMainContext().GetStringCache();

                snapshot.managedObjectCount = objectCache.GetManagedObjectCount();
                snapshot.managedObjectCap = objectCache.GetManagedObjectCap();
                snapshot.managedObjectRefs.Clear();
                if (snapshot.fetchManagedObjectRefs)
                {
                    objectCache.ForEachManagedObject(obj =>
                    {
                        snapshot.managedObjectRefs.Add(new WeakReference<object>(obj));
                    });
                }
                snapshot.jSObjectCount = objectCache.GetJSObjectCount();
                snapshot.delegateCount = objectCache.GetDelegateCount();
                snapshot.scriptValueCount = objectCache.GetScriptValueCount();
                snapshot.scriptPromiseCount = objectCache.GetScriptPromiseCount();
                snapshot.stringCount = stringCache.GetStringCount();

                var timeManager = runtime.GetTimerManager();
                snapshot.activeTimers.Clear();
                snapshot.timeNow = timeManager.now;
                timeManager.ForEach((id, delay, deadline, once) => snapshot.activeTimers.Add(new Snapshot.TimerInfo()
                {
                    id = id,
                    delay = delay,
                    deadline = deadline,
                    once = once,
                }));
            }
        }

        private string GetDescription(object obj)
        {
            try
            {
                return obj.ToString();
            }
            catch (Exception e)
            {
                return "[Error] " + e.Message;
            }
        }

        private void InspectSnapshow(Snapshot snapshot)
        {
            EditorGUILayout.BeginVertical();
            Block("JSMemoryUsage", () =>
            {
                EditorGUILayout.TextField("malloc_size", ToSizeText(snapshot.memoryUsage.malloc_size));
                EditorGUILayout.TextField("malloc_limit", ToCountText(snapshot.memoryUsage.malloc_limit));
                EditorGUILayout.TextField("memory_used_size", ToSizeText(snapshot.memoryUsage.memory_used_size));
                EditorGUILayout.TextField("malloc_count", ToCountText(snapshot.memoryUsage.malloc_count));
                EditorGUILayout.TextField("memory_used_count", ToCountText(snapshot.memoryUsage.memory_used_count));
                EditorGUILayout.TextField("atom_count", ToCountText(snapshot.memoryUsage.atom_count));
                EditorGUILayout.TextField("atom_size", ToSizeText(snapshot.memoryUsage.atom_size));
                EditorGUILayout.TextField("str_count", ToCountText(snapshot.memoryUsage.str_count));
                EditorGUILayout.TextField("str_size", ToSizeText(snapshot.memoryUsage.str_size));
                EditorGUILayout.TextField("obj_count", ToCountText(snapshot.memoryUsage.obj_count));
                EditorGUILayout.TextField("obj_size", ToSizeText(snapshot.memoryUsage.obj_size));
                EditorGUILayout.TextField("prop_count", ToCountText(snapshot.memoryUsage.prop_count));
                EditorGUILayout.TextField("prop_size", ToSizeText(snapshot.memoryUsage.prop_size));
                EditorGUILayout.TextField("shape_count", ToCountText(snapshot.memoryUsage.shape_count));
                EditorGUILayout.TextField("shape_size", ToSizeText(snapshot.memoryUsage.shape_size));
                EditorGUILayout.TextField("js_func_count", ToCountText(snapshot.memoryUsage.js_func_count));
                EditorGUILayout.TextField("js_func_size", ToSizeText(snapshot.memoryUsage.js_func_size));
                EditorGUILayout.TextField("js_func_code_size", ToSizeText(snapshot.memoryUsage.js_func_code_size));
                EditorGUILayout.TextField("js_func_pc2line_count", ToCountText(snapshot.memoryUsage.js_func_pc2line_count));
                EditorGUILayout.TextField("js_func_pc2line_size", ToSizeText(snapshot.memoryUsage.js_func_pc2line_size));
                EditorGUILayout.TextField("c_func_count", ToCountText(snapshot.memoryUsage.c_func_count));
                EditorGUILayout.TextField("array_count", ToCountText(snapshot.memoryUsage.array_count));
                EditorGUILayout.TextField("fast_array_count", ToCountText(snapshot.memoryUsage.fast_array_count));
                EditorGUILayout.TextField("fast_array_elements", ToCountText(snapshot.memoryUsage.fast_array_elements));
                EditorGUILayout.TextField("binary_object_count", ToCountText(snapshot.memoryUsage.binary_object_count));
                EditorGUILayout.TextField("binary_object_size", ToSizeText(snapshot.memoryUsage.binary_object_size));
            });

            Block("Misc.", () =>
            {
                EditorGUILayout.Toggle("Static Bind", snapshot.isStaticBinding);
                EditorGUILayout.IntField("Exported Types", snapshot.exportedTypes);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("ManagedObject Count", snapshot.managedObjectCount + "/" + snapshot.managedObjectCap, GUILayout.ExpandWidth(false));
                var pr = EditorGUILayout.GetControlRect();
                EditorGUI.DrawRect(pr, new Color(42f / 255f, 42f / 255f, 42f / 255f, 1));
                pr.x += 1f;
                pr.y += 1f;
                pr.height -= 2f;
                pr.width = (pr.width - 2f) * snapshot.managedObjectCount / snapshot.managedObjectCap;
                EditorGUI.DrawRect(pr, Color.gray);
                EditorGUILayout.EndHorizontal();
                _fetchManagedObjectRefs = EditorGUILayout.Toggle("Details", _fetchManagedObjectRefs);
                if (snapshot.fetchManagedObjectRefs)
                {
                    for (int i = 0, count = snapshot.managedObjectRefs.Count; i < count;)
                    {
                        var objRef = snapshot.managedObjectRefs[i];
                        object obj;
                        if (objRef.TryGetTarget(out obj))
                        {
                            EditorGUILayout.LabelField(i.ToString(), GetDescription(obj));
                            ++i;
                        }
                        else
                        {
                            snapshot.managedObjectRefs.RemoveAt(i);
                            --count;
                        }
                    }
                }
                EditorGUILayout.IntField("JSObject Count", snapshot.jSObjectCount);
                EditorGUILayout.IntField("Delegate Mapping Count", snapshot.delegateCount);
                EditorGUILayout.IntField("ScriptValue Mapping Count", snapshot.scriptValueCount);
                EditorGUILayout.IntField("ScriptPromise Mapping Count", snapshot.scriptPromiseCount);
                EditorGUILayout.IntField("Cached String Count", snapshot.stringCount);
            });

            Block("Timer", () =>
            {
                var count = snapshot.activeTimers.Count;
                EditorGUILayout.IntField("Active Timer", count);
                EditorGUILayout.IntField("Now", snapshot.timeNow);

                if (count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID");
                    EditorGUILayout.LabelField("Delay");
                    EditorGUILayout.LabelField("Deadline");
                    EditorGUILayout.EndHorizontal();
                    for (var i = 0; i < count; i++)
                    {
                        var t = snapshot.activeTimers[i];
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.TextField(t.id.ToString());
                        EditorGUILayout.IntField(t.delay);
                        EditorGUILayout.IntField(t.deadline);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            });
            EditorGUILayout.EndVertical();
        }

        protected override void OnUpdate()
        {
            if (_autoCap)
            {
                var rt = Time.realtimeSinceStartup;

                if (rt - _time > _timeCap)
                {
                    _time = rt;
                    CaptureAll();
                }
            }
        }

        private void CaptureAll()
        {
            for (int i = 0, count = _snapshots.Count; i < count; i++)
            {
                var snapshot = _snapshots[i];
                snapshot.alive = false;
            }
            _alive = ScriptEngine.ForEachRuntime(runtime => Capture(runtime));
            Repaint();
        }

        protected override void OnPaint()
        {
            _alive = ScriptEngine.ForEachRuntime(runtime => { });

            Block("Control", () =>
            {
                EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
                if (GUILayout.Button("Reload EditorScripting"))
                {
                    EditorRuntime.GetInstance()?.Reload();
                }

                if (GUILayout.Button("Reload CSharp"))
                {
                    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("IsDebugMode", Native.JSApi.IsDebugMode());
                EditorGUI.EndDisabledGroup();
                _autoCap = EditorGUILayout.Toggle("Auto Refresh", _autoCap);
                EditorGUI.BeginDisabledGroup(!_autoCap);
                _timeCap = EditorGUILayout.Slider("Interval", _timeCap, 1f, 30f);
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("GC (mono)"))
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                EditorGUI.BeginDisabledGroup(_alive == 0);
                if (GUILayout.Button("Capture"))
                {
                    CaptureAll();
                }
                EditorGUI.EndDisabledGroup();
            });

            if (_alive == 0)
            {
                EditorGUILayout.HelpBox("No Running Runtime", MessageType.Info);
                return;
            }

            _sv = EditorGUILayout.BeginScrollView(_sv);
            EditorGUILayout.BeginHorizontal();
            for (int i = 0, count = _snapshots.Count; i < count; i++)
            {
                var snapshot = _snapshots[i];
                if (snapshot.alive)
                {
                    lock (snapshot)
                    {
                        InspectSnapshow(snapshot);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
    }
}

#endif
