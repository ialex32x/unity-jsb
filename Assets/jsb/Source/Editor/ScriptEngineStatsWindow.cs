// using System;
// using System.IO;
// using System.Text;
// using System.Collections.Generic;

// namespace QuickJS.Editor
// {
//     using UnityEngine;
//     using UnityEditor;

//     public class ScriptEngineStatsWindow : EditorWindow
//     {
//         [MenuItem("JS Bridge/Stats Viewer")]
//         static void OpenThis()
//         {
//             GetWindow<ScriptEngineStatsWindow>().Show();
//         }

//         void OnEnable()
//         {
//             titleContent = new GUIContent("ScriptEngine Stats");
//         }

//         void OnGUI()
//         {
//             var vm = DuktapeVM.GetInstance();

//             if (vm == null)
//             {
//                 EditorGUILayout.HelpBox("No Running VM", MessageType.Info);
//                 return;
//             }

//             uint objectCount;
//             uint allocBytes;
//             uint poolBytes;
//             vm.GetMemoryState(out objectCount, out allocBytes, out poolBytes);
//             EditorGUILayout.IntField("Objects", (int)objectCount);
//             if (allocBytes > 1024 * 1024 * 2)
//             {
//                 EditorGUILayout.FloatField("Allocated Memory (MB)", (float)allocBytes / 1024f / 1024f);
//             }
//             else if (allocBytes > 1024 * 2)
//             {
//                 EditorGUILayout.FloatField("Allocated Memory (KB)", (float)allocBytes / 1024f);
//             }
//             else
//             {
//                 EditorGUILayout.IntField("Allocated Memory", (int)allocBytes);
//             }

//             if (poolBytes != 0)
//             {
//                 EditorGUILayout.IntField("Pool Size", (int)poolBytes);
//                 EditorGUILayout.FloatField("Used (%)", (float)allocBytes * 100f / poolBytes);
//             }

//             EditorGUILayout.IntField("Exported Types", vm.GetExportedTypeCount());

//             var objectCache = vm.GetObjectCache();
//             EditorGUILayout.IntField("ManagedObject Count", objectCache.GetManagedObjectCount());
//             EditorGUILayout.IntField("JSObject Count", objectCache.GetJSObjectCount());
//             EditorGUILayout.IntField("Delegate Count", objectCache.GetDelegateCount());

//             var scheduler = DuktapeRunner.GetScheduler();
//             if (scheduler != null)
//             {
//                 EditorGUILayout.IntField("Active Timer", scheduler.GetActiveTimeHandleCount());
//             }
//         }
//     }
// }
