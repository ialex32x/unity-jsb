using System.Runtime.InteropServices;

namespace QuickJS.Native
{
    /*
        typedef struct JSPropertyDescriptor {
            int flags;
            JSValue value;
            JSValue getter;
            JSValue setter;
        } JSPropertyDescriptor;
    */
    [StructLayout(LayoutKind.Sequential)]
    public struct JSPropertyDescriptor
    {
        public int flags;
        public JSValue value;
        public JSValue getter;
        public JSValue setter;
    }
}