
#if defined(JSB_F_WINDOWS)
    #if defined(JSB_COMPILING)
        #define JSB_EXTERNAL_DECL extern __declspec(dllexport)
        #define JSB_EXTERNAL __declspec(dllexport)
    #else
        #define JSB_EXTERNAL_DECL extern __declspec(dllimport)
        #define JSB_EXTERNAL
    #endif
#else
    #define JSB_EXTERNAL_DECL  __attribute__ ((visibility("default"))) extern
    #define JSB_EXTERNAL       __attribute__ ((visibility("default")))
#endif

JSB_EXTERNAL_DECL JSRuntime *XJS_NewRuntime();
JSB_EXTERNAL_DECL void XJS_FreeRuntime(JSRuntime *rt);
JSB_EXTERNAL_DECL JSContext *XJS_NewContext(JSRuntime *rt);
JSB_EXTERNAL_DECL void XJS_FreeContext(JSContext *s);
