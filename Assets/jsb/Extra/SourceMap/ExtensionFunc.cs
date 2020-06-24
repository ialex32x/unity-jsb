using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS
{
    public static class ExtensionFunc
    {
        public static Extra.SourceMapHelper EnableSourceMap(this ScriptRuntime runtime, string workspace = null)
        {
            var helper = new Extra.SourceMapHelper();
            helper.OpenWorkspace(runtime, workspace);
            return helper;
        }
    }
}
