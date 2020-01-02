
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

struct JSBRuntime;
struct JSBValue;
struct JSBClass;

typedef void JSBClassFinalizer(struct JSBRuntime *vm, struct JSBValue *val);

struct JSBClass {
    JSClassID class_id;
    // JSValue prototype;
    // JSValue constructor;
    JSClassDef class_def;
};

struct JSBRuntime {
    struct JSBRuntime *_next;
    JSRuntime *rt;
    JSContext *ctx;

    struct JSBClass origin;
    JSBClassFinalizer *finalizer;
};

struct JSBValue {
    int32_t refid;
};

JSB_EXTERNAL_DECL struct JSBRuntime *JSB_NewRuntime(JSBClassFinalizer *finalizer);
JSB_EXTERNAL_DECL void JSB_FreeRuntime(struct JSBRuntime *vm);

JSB_EXTERNAL_DECL JSContext *JSB_NewContext();
