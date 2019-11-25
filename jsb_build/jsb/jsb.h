
#if defined(JSB_COMPILING)
#define JSB_EXTERNAL_DECL extern __declspec(dllexport)
#define JSB_EXTERNAL __declspec(dllexport)
#else
#define JSB_EXTERNAL_DECL extern __declspec(dllimport)
#define JSB_EXTERNAL
#endif

JSB_EXTERNAL_DECL void init();
