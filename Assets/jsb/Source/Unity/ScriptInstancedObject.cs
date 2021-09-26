#if !JSB_UNITYLESS

namespace QuickJS.Unity
{
    using Native;

    public interface IScriptInstancedObject
    {
        int IsInstanceOf(JSValue ctor);
        JSValue CloneValue();
    }
}

#endif // JSB_UNITYLESS
