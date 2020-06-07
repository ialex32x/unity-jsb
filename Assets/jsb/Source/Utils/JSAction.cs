using QuickJS.Native;

namespace QuickJS.Utils
{
    public delegate void JSActionCallback(ScriptRuntime runtime, JSValue value);
    
    public struct JSAction
    {
        public JSValue value;
        public JSActionCallback callback;
    }
}