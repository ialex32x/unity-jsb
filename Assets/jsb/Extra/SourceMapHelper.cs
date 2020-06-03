// using System;
// using System.IO;
// using System.Text;
// using System.Collections;
// using System.Collections.Generic;

// namespace Duktape
// {
//     using UnityEngine;

//     using SourcePosition = SourcemapToolkit.SourcemapParser.SourcePosition;
//     using SourceMap = SourcemapToolkit.SourcemapParser.SourceMap;
//     using SourceMapParser = SourcemapToolkit.SourcemapParser.SourceMapParser;

//     [JSAutoRun]
//     public static class SourceMapHelper
//     {
//         [Serializable]
//         public class DukConfig
//         {
//             public string workspace;
//         }

//         [Serializable]
//         public class TSConfig
//         {
//             [Serializable]
//             public class CompilerOptions
//             {
//                 public string module;
//                 public string target;
//                 public string sourceRoot;
//                 public string outDir;
//                 public string outFile;
//                 public string[] typeRoots;
//                 public string moduleResolution;
//                 public string[] types;
//                 public bool listEmittedFiles;
//                 public bool experimentalDecorators;
//                 public bool noImplicitAny;
//                 public bool allowJs;
//                 public bool inlineSourceMap;
//                 public bool sourceMap;
//             }
//             public CompilerOptions compilerOptions;
//             public bool compileOnSave;
//             public string[] include;
//             public string[] exclude;
//         }

//         private static string _sourceRoot = "";
//         private static SourcePosition _shared = new SourcePosition();
//         private static Dictionary<string, SourceMap> _sourceMaps = new Dictionary<string, SourceMap>();

//         private static SourceMap GetSourceMap(IntPtr ctx, string fileName)
//         {
//             SourceMap sourceMap;
//             if (!_sourceMaps.TryGetValue(fileName, out sourceMap))
//             {
//                 try
//                 {
//                     var fileResolver = DuktapeVM.GetVM(ctx).fileResolver;
//                     var fileContent = fileResolver.ReadAllBytes(fileName + ".map");
//                     if (fileContent != null && fileContent.Length > 0)
//                     {
//                         using (var stream = new MemoryStream(fileContent))
//                         {
//                             var parser = new SourceMapParser();
//                             var reader = new StreamReader(stream);
//                             sourceMap = parser.ParseSourceMap(reader);
//                             // Debug.Log($"[SourceMapHelper] parse sourceMap: {sourceMap.File} ({resolvedPath})");
//                         }
//                     }
//                 }
//                 finally
//                 {
//                     _sourceMaps[fileName] = sourceMap;
//                 }
//             }
//             return sourceMap;
//         }

//         private static string duk_source_position(IntPtr ctx, string funcName, string fileName, int lineNumber)
//         {
//             if (lineNumber != 0)
//             {
//                 try
//                 {
//                     var sourceMap = GetSourceMap(ctx, fileName);
//                     if (sourceMap != null)
//                     {
//                         var pos = _shared;
//                         pos.ZeroBasedLineNumber = lineNumber;
//                         pos.ZeroBasedColumnNumber = 0;
//                         var entry = sourceMap.GetMappingEntryForGeneratedSourcePosition(pos);
//                         if (entry != null)
//                         {
//                             var entryPos = entry.OriginalSourcePosition;
//                             var resolvedOriginal = Path.Combine(_sourceRoot, entry.OriginalFileName).Replace('\\', '/');
//                             if (string.IsNullOrEmpty(funcName))
//                             {
//                                 funcName = "anonymous";
//                             }
//                             return $"typescript:{funcName}() (at {resolvedOriginal}:{entryPos.ZeroBasedLineNumber + 1})";
//                         }
//                     }
//                 }
//                 catch (Exception exception)
//                 {
//                     Debug.LogError($"failed to parse source map [{fileName}]: {exception}");
//                 }
//                 if (string.IsNullOrEmpty(funcName))
//                 {
//                     funcName = "[anonymous]";
//                 }
//                 return $"{funcName} ({fileName}:{lineNumber})";
//             }
//             if (string.IsNullOrEmpty(funcName))
//             {
//                 funcName = "[anonymous]";
//             }
//             return $"{funcName} (<native code>)";
//         }

//         // 剔除行注释
//         private static string NormalizeJson(string json)
//         {
//             var outstr = new StringBuilder();
//             var state = 0;
//             for (int i = 0; i < json.Length; i++)
//             {
//                 if (state == 0)
//                 {
//                     if (json[i] == '/')
//                     {
//                         state = 1;
//                         continue;
//                     }
//                 }
//                 else if (state == 1)
//                 {
//                     if (json[i] == '/')
//                     {
//                         state = 2;
//                         continue;
//                     }
//                     state = 0;
//                     outstr.Append('/');
//                 }
//                 else if (state == 2)
//                 {
//                     if (json[i] != '\n')
//                     {
//                         continue;
//                     }
//                     state = 0;
//                 }
//                 outstr.Append(json[i]);
//             }
//             return outstr.ToString();
//         }

//         public static void Run()
//         {
//             var workspace = "";
//             if (File.Exists("duktape.json"))
//             {
//                 var text = NormalizeJson(File.ReadAllText("duktape.json"));
//                 var dukconfig = JsonUtility.FromJson<DukConfig>(text);

//                 workspace = dukconfig.workspace;
//             }
//             var tsconfigPath = string.IsNullOrEmpty(workspace) ? "tsconfig.json" : Path.Combine(workspace, "tsconfig.json");
//             if (File.Exists(tsconfigPath))
//             {
//                 var text = NormalizeJson(File.ReadAllText(tsconfigPath));
//                 var tsconfig = JsonUtility.FromJson<TSConfig>(text);
//                 var sourceRoot = tsconfig.compilerOptions.sourceRoot;

//                 if (string.IsNullOrEmpty(workspace) || Path.IsPathRooted(sourceRoot))
//                 {
//                     _sourceRoot = sourceRoot;
//                 }
//                 else
//                 {
//                     _sourceRoot = Path.Combine(workspace, sourceRoot);
//                 }
//                 DuktapeAux.duk_source_position = duk_source_position;
//                 // Debug.Log($"[SourceMapHelper] enabled {_sourceRoot}");
//             }
//             else
//             {
//                 Debug.LogWarning("no tsconfig.json found");
//             }
//         }
//     }
// }
