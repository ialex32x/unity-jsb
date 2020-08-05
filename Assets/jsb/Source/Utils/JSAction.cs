using QuickJS.Native;
using System.Runtime.InteropServices;

namespace QuickJS.Utils
{
    public delegate void JSActionCallback(ScriptRuntime runtime, JSAction value);
    
    // [StructLayout(LayoutKind.Sequential)]
    public struct JSAction
    {
        public JSValue value;
        public JSActionCallback callback;

        // for worker only 
        public JSWorker worker;
        public IO.ByteBuffer buffer;
    }
}