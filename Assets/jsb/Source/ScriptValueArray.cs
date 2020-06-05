using System;
using AOT;
using QuickJS.Native;

namespace QuickJS
{
    using UnityEngine;

    public class ScriptValueArray : ScriptValue
    {
        public ScriptValueArray(ScriptContext context, JSValue jsValue)
        : base(context, jsValue)
        {
        }
    }
}
