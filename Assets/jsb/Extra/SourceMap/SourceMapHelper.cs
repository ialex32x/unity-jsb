using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS.Extra
{
    using UnityEngine;
    using Native;
    using Utils;

    using SourcePosition = SourcemapToolkit.SourcemapParser.SourcePosition;
    using SourceMap = SourcemapToolkit.SourcemapParser.SourceMap;
    using SourceMapParser = SourcemapToolkit.SourcemapParser.SourceMapParser;

    public class SourceMapHelper
    {
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
                    var fileResolver = runtime.GetPathResolver();
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
            try
            {
                var sourceMap = GetSourceMap(ctx, fileName);
                if (sourceMap != null)
                {
                    var pos = _shared;
                    pos.ZeroBasedLineNumber = lineNumber - 1;
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

        public void OpenSourceRoot(ScriptRuntime runtime, string sourceRoot)
        {
            _sourceRoot = sourceRoot;
            runtime.OnSourceMap = js_source_position;
        }

        public void OpenWorkspace(ScriptRuntime runtime, string workspace)
        {
            var tsconfigPath = string.IsNullOrEmpty(workspace) ? "tsconfig.json" : Path.Combine(workspace, "tsconfig.json");
            if (File.Exists(tsconfigPath))
            {
                var text = Utils.TextUtils.NormalizeJson(File.ReadAllText(tsconfigPath));
                var tsconfig = JsonUtility.FromJson<TSConfig>(text);
                var sourceRoot = tsconfig.compilerOptions.sourceRoot;

                if (string.IsNullOrEmpty(workspace) || Path.IsPathRooted(sourceRoot))
                {
                    OpenSourceRoot(runtime, sourceRoot);
                }
                else
                {
                    OpenSourceRoot(runtime, Path.Combine(workspace, sourceRoot));
                }
            }
            else
            {
                Debug.LogWarning("no tsconfig.json found");
            }
        }
    }
}
