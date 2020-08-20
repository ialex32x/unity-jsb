using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class PlainClassCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public PlainClassCodeGen(CodeGenerator cg, string name)
        {
            this.cg = cg;
            this.cg.cs.AppendLine("public partial class {0}", name);
            this.cg.cs.AppendLine("{");
            this.cg.cs.AddTabLevel();
        }

        public void Dispose()
        {
            this.cg.cs.DecTabLevel();
            this.cg.cs.AppendLine("}");
        }
    }
}
