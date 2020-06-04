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
static JSClassID js_bridge_class_id = 0;

// quickjs 内置 class id 之后的第一个可用 id
JSClassID JSB_GetClassID()
{
    return js_class_id_begin;
}

JSClassID JSB_GetBridgeClassID()
{
    return js_bridge_class_id;
}

#define JS_BO_TYPE 1
#define JS_BO_OBJECT 2
#define JS_BO_VALUE 3

typedef struct JSPayloadHeader {
    int32_t type_id; // JS_BO_*
    int32_t value;
} JSPayloadHeader;

static JSPayloadHeader _null_payload = { .type_id = 0, .value = 0 };

typedef struct JSPayload {
    JSPayloadHeader header;
    char data[1];
} JSPayload;

JSValue JSB_NewBridgeObject(JSContext *ctx, JSValue proto, int32_t value)
{
    JSValue obj = JS_NewObjectProtoClass(ctx, proto, js_bridge_class_id);
    if (!JS_IsException(obj))
    {
        JSPayload *sv = (JSPayload *) js_malloc(ctx, sizeof(JSPayloadHeader));
        sv->header.type_id = JS_BO_OBJECT;
        sv->header.value = value;
        JS_SetOpaque(obj, sv);
    }

    return obj;
}

JSValue JSB_NewBridgeValue(JSContext *ctx, JSValue proto, int32_t size)
{
    JSValue obj = JS_NewObjectProtoClass(ctx, proto, js_bridge_class_id);
    if (!JS_IsException(obj))
    {
        JSPayload *sv = (JSPayload *) js_mallocz(ctx, sizeof(JSPayloadHeader) + size);
        sv->header.type_id = JS_BO_VALUE;
        sv->header.value = size;
        JS_SetOpaque(obj, sv);
    }

    return obj;
}

void JSB_SetBridgeType(JSContext *ctx, JSValue obj, int32_t type)
{
    JSPayload *sv = (JSPayload *) js_mallocz(ctx, sizeof(JSPayloadHeader));
    sv->header.type_id = JS_BO_TYPE;
    sv->header.value = type;
    JS_SetOpaque(obj, sv);
}

// 释放数据, 返回头信息副本
JSPayloadHeader JSB_FreePayload(JSContext *ctx, JSValue val)
{
    void *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv) 
    {
        JSPayloadHeader header = *(JSPayloadHeader *)sv;
        JS_SetOpaque(val, NULL);
        js_free(ctx, sv);
        return header;
    }
    return _null_payload;
}

JSPayloadHeader JSB_FreePayloadRT(JSRuntime *rt, JSValue val)
{
    void *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv) 
    {
        JSPayloadHeader header = *(JSPayloadHeader *)sv;
        JS_SetOpaque(val, NULL);
        js_free_rt(rt, sv);
        return header;
    }
    return _null_payload;
}

JSPayloadHeader jsb_get_payload_header(JSValue val)
{
    JSPayloadHeader *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv) 
    {
        return *sv;
    }
    return _null_payload;
}

JSPayload *jsb_get_payload(JSValue val)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv) 
    {
        return sv;
    }
    return 0;
}

void jsb_get_floats(JSPayload *sv, int n, float *v0) 
{
    float *ptr = (float *)&(sv->data[0]);
    for (int i = 0; i < n; ++i) 
    {
        *(v0 + i) = ptr[0];
    }
}

void jsb_set_floats(JSPayload *sv, int n, float *v0) 
{
    float *ptr = (float *)&(sv->data[0]);
    for (int i = 0; i < n; ++i) 
    {
        ptr[i] = *(v0 + i);
    }
}

void jsb_get_float_2(JSPayload *sv, float *v0, float *v1) 
{
    float *ptr = (float *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
}

void jsb_set_float_2(JSPayload *sv, float v0, float v1) 
{
    float *ptr = (float *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
}

void jsb_get_float_3(JSPayload *sv, float *v0, float *v1, float *v2) 
{
    float *ptr = (float *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
    *v2 = ptr[2];
}

void jsb_set_float_3(JSPayload *sv, float v0, float v1, float v2) 
{
    float *ptr = (float *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
    ptr[2] = v2;
}

void jsb_get_float_4(JSPayload *sv, float *v0, float *v1, float *v2, float *v3) 
{
    float *ptr = (float *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
    *v2 = ptr[2];
    *v3 = ptr[3];
}

void jsb_set_float_4(JSPayload *sv, float v0, float v1, float v2, float v3) 
{
    float *ptr = (float *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
    ptr[2] = v2;
    ptr[3] = v3;
}

void jsb_get_int_1(JSPayload *sv, int *v0) 
{
    int *ptr = (int *)&(sv->data[0]);
    *v0 = ptr[0];
}

void jsb_set_int_1(JSPayload *sv, int v0) 
{
    int *ptr = (int *)&(sv->data[0]);
    ptr[0] = v0;
}

void jsb_get_int_2(JSPayload *sv, int *v0, int *v1) 
{
    int *ptr = (int *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
}

void jsb_set_int_2(JSPayload *sv, int v0, int v1) 
{
    int *ptr = (int *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
}

void jsb_get_int_3(JSPayload *sv, int *v0, int *v1, int *v2) 
{
    int *ptr = (int *)&(sv->data[0]);
    *v0 = ptr[0];
    *v1 = ptr[1];
    *v2 = ptr[2];
}

void jsb_set_int_3(JSPayload *sv, int v0, int v1, int v2) 
{
    int *ptr = (int *)&(sv->data[0]);
    ptr[0] = v0;
    ptr[1] = v1;
    ptr[2] = v2;
}

void JSB_Init()
{
    if (js_class_id_begin == 0) 
    {
        JS_NewClassID(&js_bridge_class_id);
        JS_NewClassID(&js_class_id_begin);
    }
}
