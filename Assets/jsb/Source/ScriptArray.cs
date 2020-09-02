using System;
using QuickJS.Native;

namespace QuickJS
{
    using UnityEngine;

    public class ScriptArray : ScriptValue
    {
        public ScriptArray(ScriptContext context, JSValue jsValue)
        : base(context, jsValue)
        {
        }
    }
}
