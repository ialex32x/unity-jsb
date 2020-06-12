using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS.Extra
{
    using UnityEngine;
    using Native;

    using SourcePosition = SourcemapToolkit.SourcemapParser.SourcePosition;
    using SourceMap = SourcemapToolkit.SourcemapParser.SourceMap;
    using SourceMapParser = SourcemapToolkit.SourcemapParser.SourceMapParser;

    public class SourceMapHelper
    {
        [Serializable]
        public class TSConfig
        {
            [Serializable]
            public class CompilerOptions
            {
                public string module;
                public string target;
                public string sourceRoot;
                public string outDir;
                public string outFile;
                public string[] typeRoots;
                public string moduleResolution;
                public string[] types;
                public bool listEmittedFiles;
                public bool experimentalDecorators;
                public bool noImplicitAny;
                public bool allowJs;
                public bool inlineSourceMap;
                public bool sourceMap;
            }
            public CompilerOptions compilerOptions;
            public bool compileOnSave;
            public string[] include;
            public string[] exclude;
        }

        private string _sourceRoot = "";
        private SourcePosition _shared = new SourcePosition();
        private Dictionary<string, SourceMap> _sourceMaps = new Dictionary<string, SourceMap>();

        private SourceMap GetSourceMap(JSContext ctx, string fileName)
        {
            SourceMap sourceMap;
            if (!_sourceMaps.TryGetValue(fileName, out sourceMap))
            {
                try
                {
                    var runtime = ScriptEngine.GetRuntime(ctx);
                    var fileResolver = runtime.GetFileResolver();
                    var fileSystem = runtime.GetFileSystem();
                    string resolvedPath;
                    if (fileResolver.ResolvePath(fileSystem, fileName + ".map", out resolvedPath))
                    {
                        var fileContent = fileSystem.ReadAllBytes(resolvedPath);
                        if (fileContent != null && fileContent.Length > 0)
                        {
                            using (var stream = new MemoryStream(fileContent))
                            {
                                var parser = new SourceMapParser();
                                var reader = new StreamReader(stream);
                                sourceMap = parser.ParseSourceMap(reader);
                                // Debug.Log($"[SourceMapHelper] parse sourceMap: {sourceMap.File} ({resolvedPath})");
                            }
                        }
                    }
                }
                finally
                {
                    _sourceMaps[fileName] = sourceMap;
                }
            }
            return sourceMap;
        }

        private string js_source_position(JSContext ctx, string funcName, string fileName, int lineNumber)
        {
            if (lineNumber != 0)
            {
                try
                {
                    var sourceMap = GetSourceMap(ctx, fileName);
                    if (sourceMap != null)
                    {
                        var pos = _shared;
                        pos.ZeroBasedLineNumber = lineNumber;
                        pos.ZeroBasedColumnNumber = 0;
                        var entry = sourceMap.GetMappingEntryForGeneratedSourcePosition(pos);
                        if (entry != null)
                        {
                            var entryPos = entry.OriginalSourcePosition;
                            var resolvedOriginal = Path.Combine(_sourceRoot, entry.OriginalFileName).Replace('\\', '/');
                            if (string.IsNullOrEmpty(funcName))
                            {
                                funcName = "anonymous";
                            }
                            return $"typescript:{funcName}() (at {resolvedOriginal}:{entryPos.ZeroBasedLineNumber + 1})";
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"failed to parse source map [{fileName}]: {exception}");
                }
                if (string.IsNullOrEmpty(funcName))
                {
                    funcName = "[anonymous]";
                }
                return $"{funcName} ({fileName}:{lineNumber})";
            }
            if (string.IsNullOrEmpty(funcName))
            {
                funcName = "[anonymous]";
            }
            return $"{funcName} (<native code>)";
        }

        public void Open(string workspace = null)
        {
            var tsconfigPath = string.IsNullOrEmpty(workspace) ? "tsconfig.json" : Path.Combine(workspace, "tsconfig.json");
            if (File.Exists(tsconfigPath))
            {
                var text = Utils.TextUtils.NormalizeJson(File.ReadAllText(tsconfigPath));
                var tsconfig = JsonUtility.FromJson<TSConfig>(text);
                var sourceRoot = tsconfig.compilerOptions.sourceRoot;

                if (string.IsNullOrEmpty(workspace) || Path.IsPathRooted(sourceRoot))
                {
                    _sourceRoot = sourceRoot;
                }
                else
                {
                    _sourceRoot = Path.Combine(workspace, sourceRoot);
                }
                //TODO: quickjs stacktrace
                // DuktapeAux.js_source_position = js_source_position;
                // Debug.Log($"[SourceMapHelper] enabled {_sourceRoot}");
            }
            else
            {
                Debug.LogWarning("no tsconfig.json found");
            }
        }
    }
}
