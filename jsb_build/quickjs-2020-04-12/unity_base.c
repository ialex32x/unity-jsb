/*
vim /etc/apt/sources.list
deb http://us.archive.ubuntu.com/ubuntu trusty main universe
sudo apt-get update
sudo apt-get install mingw32
    sudo apt-get install mingw-w64
./configure --host=i686-w64-mingw32
    ./configure --host=x86_64-w64-mingw32
make
*/

#include "quickjs.h"

#ifndef FALSE
enum
{
    FALSE = 0,
    TRUE = 1,
};
#endif

enum
{
    JS_ATOM_NULL,
#define DEF(name, str) JS_ATOM_##name,
#include "quickjs-atom.h"
#undef DEF
    JS_ATOM_END,
};

#define DEF(name, str) JSAtom JSB_ATOM_##name() { return JS_ATOM_##name; }
#include "quickjs-atom.h"
#undef DEF

int JSB_ToUint32(JSContext *ctx, uint32_t *pres, JSValueConst val)
{
    return JS_ToInt32(ctx, (int32_t*)pres, val);
}

uint32_t JSB_ToUint32z(JSContext *ctx, JSValueConst val)
{
    uint32_t pres = 0;
    JS_ToInt32(ctx, (int32_t*)&pres, val);
    return pres;
}

JSValue JSB_NewFloat64(JSContext *ctx, double d)
{
    return JS_NewFloat64(ctx, d);
}

JSValue JSB_ThrowTypeError(JSContext *ctx, const char *msg)
{
    return JS_ThrowTypeError(ctx, "%s", msg);
}

JSValue JSB_ThrowInternalError(JSContext *ctx, const char *msg)
{
    return JS_ThrowInternalError(ctx, "%s", msg);
}

JSValue JSB_ThrowRangeError(JSContext *ctx, const char *msg)
{
    return JS_ThrowRangeError(ctx, "%s", msg);
}

JSValue JSB_ThrowReferenceError(JSContext *ctx, const char *msg)
{
    return JS_ThrowReferenceError(ctx, "%s", msg);
}

void JSB_FreeValue(JSContext *ctx, JSValue v)
{
    JS_FreeValue(ctx, v);
}

void JSB_FreeValueRT(JSRuntime *rt, JSValue v)
{
    JS_FreeValueRT(rt, v);
}

JSValue JSB_DupValue(JSContext *ctx, JSValueConst v)
{
    return JS_DupValue(ctx, v);
}

JSValue JSB_DupValueRT(JSRuntime *rt, JSValueConst v)
{
    return JS_DupValueRT(rt, v);
}

JSValue JSB_Eval(JSContext *ctx, const char *input, int input_len,
                 const char *filename, int eval_flags)
{
    return JS_Eval(ctx, input, input_len, filename, eval_flags);
}

JSValue JSB_NewPropertyObjectStr(JSContext *ctx, JSValueConst this_obj, const char *name, int flags)
{
    JSValue p = JS_GetPropertyStr(ctx, this_obj, name);
    if (JS_IsObject(p))
    {
        return p;
    }
    JS_FreeValue(ctx, p);
    p = JS_NewObject(ctx);
    JS_DupValue(ctx, p);
    JS_DefinePropertyValueStr(ctx, this_obj, name, p, flags);
    return p;
}

JSClassID JSB_NewClass(JSRuntime *rt, JSClassID class_id, const char *class_name, JSClassFinalizer *finalizer)
{
    if (JS_IsRegisteredClass(rt, class_id))
    {
        return 0;
    }
    JSClassDef cls_def;

    cls_def.class_name = class_name;
    cls_def.finalizer = finalizer;
    cls_def.exotic = NULL;
    cls_def.gc_mark = NULL;
    cls_def.call = NULL;

    JS_NewClassID(&class_id);
    JS_NewClass(rt, class_id, &cls_def);
    return class_id;
}

static JSClassID js_class_id_begin = 0;

// quickjs 内置 class id 之后的第一个可用 id
JSClassID JSB_GetClassID()
{
    if (js_class_id_begin == 0) 
    {
        JS_NewClassID(&js_class_id_begin);
    }
    return js_class_id_begin;
}

typedef struct JSPayloadHeader {
    int32_t type_id; // registered type id (exported types in csharp)
    union {
        int32_t object_id;
        uint32_t size;
    };
} JSPayloadHeader;

static JSPayloadHeader _null_payload = { .type_id = 0, .object_id = 0 };

typedef struct JSTypePayload {
    JSPayloadHeader header; 
} JSTypePayload;

typedef struct JSClassPayload {
    JSPayloadHeader header; 
} JSClassPayload;

typedef struct JSStructPayload {
    JSPayloadHeader header;
    char data[1];
} JSStructPayload;

void JSB_NewTypePayload(JSContext *ctx, JSValue val, JSClassID class_id, int32_t type_id)
{
    JSTypePayload *sv = (JSTypePayload *) js_malloc(ctx, sizeof(JSTypePayload));
    sv->header.type_id = type_id;
    JS_SetOpaque(val, sv);
}

void JSB_NewClassPayload(JSContext *ctx, JSValue val, JSClassID class_id, int32_t type_id, int32_t object_id)
{
    JSClassPayload *sv = (JSClassPayload *) js_malloc(ctx, sizeof(JSClassPayload));
    sv->header.type_id = type_id;
    sv->header.object_id = object_id;
    JS_SetOpaque(val, sv);
}

void JSB_NewStructPayload(JSContext *ctx, JSValue val, JSClassID class_id, int32_t type_id, int32_t object_id, uint32_t size)
{
    JSStructPayload *sv = (JSStructPayload *) js_malloc(ctx, sizeof(JSPayloadHeader) + size);
    sv->header.type_id = type_id;
    sv->header.size = size;
    JS_SetOpaque(val, sv);
}

JSPayloadHeader JSB_FreePayload(JSContext *ctx, JSValue val, JSClassID class_id)
{
    void *sv = JS_GetOpaque(val, class_id);
    if (sv) 
    {
        JSPayloadHeader header = *(JSPayloadHeader *)sv;
        JS_SetOpaque(val, NULL);
        js_free(ctx, sv);
        return header;
    }
    return _null_payload;
}

JSPayloadHeader JSB_FreePayloadRT(JSRuntime *rt, JSValue val, JSClassID class_id)
{
    void *sv = JS_GetOpaque(val, class_id);
    if (sv) 
    {
        JSPayloadHeader header = *(JSPayloadHeader *)sv;
        JS_SetOpaque(val, NULL);
        js_free_rt(rt, sv);
        return header;
    }
    return _null_payload;
}

JSPayloadHeader jsb_get_payload(JSValue val, JSClassID class_id)
{
    JSPayloadHeader *sv = JS_GetOpaque(val, class_id);
    if (sv) 
    {
        return *sv;
    }
    return _null_payload;
}

void jsb_get_floats(JSStructPayload *sv, int n, float *v0) 
{
    float *ptr = (float *)&(sv->data[0]);
    for (int i = 0; i < n; ++i) 
    {
        *(v0 + i) = ptr[0];
    }
}

void jsb_set_floats(JSStructPayload *sv, int n, float *v0) 
{
    float *ptr = (float *)&(sv->data[0]);
    for (int i = 0; i < n; ++i) 
    {
        ptr[i] = *(v0 + i);
    }
}

void jsb_get_float_2(JSStructPayload *sv, float *v0, float *v1) 
{
    float *ptr = (float *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
}

void jsb_set_float_2(JSStructPayload *sv, float v0, float v1) 
{
    float *ptr = (float *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
}

void jsb_get_float_3(JSStructPayload *sv, float *v0, float *v1, float *v2) 
{
    float *ptr = (float *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
    *v2 = ptr[2];
}

void jsb_set_float_3(JSStructPayload *sv, float v0, float v1, float v2) 
{
    float *ptr = (float *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
    ptr[2] = v2;
}

void jsb_get_float_4(JSStructPayload *sv, float *v0, float *v1, float *v2, float *v3) 
{
    float *ptr = (float *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
    *v2 = ptr[2];
    *v3 = ptr[3];
}

void jsb_set_float_4(JSStructPayload *sv, float v0, float v1, float v2, float v3) 
{
    float *ptr = (float *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
    ptr[2] = v2;
    ptr[3] = v3;
}

void jsb_get_int_1(JSStructPayload *sv, int *v0) 
{
    int *ptr = (int *)&(sv->data[0]);
    *v0 = ptr[0];
}

void jsb_set_int_1(JSStructPayload *sv, int v0) 
{
    int *ptr = (int *)&(sv->data[0]);
    ptr[0] = v0;
}

void jsb_get_int_2(JSStructPayload *sv, int *v0, int *v1) 
{
    int *ptr = (int *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
}

void jsb_set_int_2(JSStructPayload *sv, int v0, int v1) 
{
    int *ptr = (int *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
}

void jsb_get_int_3(JSStructPayload *sv, int *v0, int *v1, int *v2) 
{
    int *ptr = (int *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
    *v2 = ptr[2];
}

void jsb_set_int_3(JSStructPayload *sv, int v0, int v1, int v2) 
{
    int *ptr = (int *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
    ptr[2] = v2;
}

void JSB_Init()
{
}
