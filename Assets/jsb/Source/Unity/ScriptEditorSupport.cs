#if !JSB_UNITYLESS

namespace QuickJS.Unity
{
    using Native;

    public interface IScriptEditorSupport
    {
        bool isStandaloneScript { get; }

        bool isScriptInstanced { get; }

        JSScriptRef scriptRef { get; set; }

        JSContext ctx { get; }

        JSScriptProperties properties { get; }

        bool IsValid();

        JSValue ToValue();

        void ReleaseScriptInstance();

        bool CreateScriptInstance();
    }
}

#endif
