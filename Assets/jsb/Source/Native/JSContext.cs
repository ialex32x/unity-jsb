using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSContext
    {
        private unsafe void* _ctx;

        public void SetProperty(JSValue this_obj, string name, JSCFunction fn, int length = 0)
        {
            JSApi.JS_SetPropertyStr(this, this_obj, name, JSApi.JS_NewCFunction(this, fn, name, length));
        }

        public override unsafe int GetHashCode()
        {
            return (int) _ctx;
        }
    }
}
