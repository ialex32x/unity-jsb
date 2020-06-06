using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptDelegate : ScriptValue
    {
        public Delegate target;

        public ScriptDelegate(ScriptContext context, JSValue jsValue) : base(context, jsValue)
        {
        }
    }
}
