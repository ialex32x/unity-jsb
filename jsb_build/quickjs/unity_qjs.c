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

#ifndef UNITY_WEBGL
#include "quickjs.h"
#endif

#ifndef EMSCRIPTEN
#ifndef CONFIG_ATOMICS
#define CONFIG_ATOMICS
#endif
#endif

#define byte unsigned char
// #define JS_HIDDEN_PROP(s) ("\xFF" s)

#ifndef UNITY_WEBGL

#ifndef FALSE
enum
{
    FALSE = 0,
    TRUE = 1,
};
#endif // !FALSE

enum
{
    __JS_ATOM_NULL = JS_ATOM_NULL,
#define DEF(name, str) JS_ATOM_##name,
#include "quickjs-atom.h"
#undef DEF
    JS_ATOM_END,
};

#define DEF(name, str) \
    JSAtom JSB_ATOM_##name() { return JS_ATOM_##name; }
#include "quickjs-atom.h"
#undef DEF

#endif // !UNITY_WEBGL

JSValue JSB_NewEmptyString(JSContext *ctx)
{
    return JS_NewStringLen(ctx, "", 0);
}

JSValue JSB_NewInt64(JSContext *ctx, int64_t val)
{
    return JS_NewInt64(ctx, val);
}

int JSB_ToUint32(JSContext *ctx, uint32_t *pres, JSValueConst val)
{
    return JS_ToInt32(ctx, (int32_t *)pres, val);
}

uint32_t JSB_ToUint32z(JSContext *ctx, JSValueConst val)
{
    uint32_t pres = 0;
    JS_ToInt32(ctx, (int32_t *)&pres, val);
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

JSValue JSB_NewCFunction(JSContext *ctx, JSCFunction *func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic)
{
    const char *name = JS_AtomToCString(ctx, atom);
    if (!name)
    {
        return JS_ThrowInternalError(ctx, "no such atom: %d", atom);
    }
    JSValue funcVal = JS_NewCFunction2(ctx, func, name, length, cproto, magic);
    JS_FreeCString(ctx, name);
    return funcVal;
}

JSValue JSB_NewCFunctionMagic(JSContext *ctx, JSCFunctionMagic *func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic)
{
    const char *name = JS_AtomToCString(ctx, atom);
    if (!name)
    {
        return JS_ThrowInternalError(ctx, "no such atom: %d", atom);
    }
    JSValue funcVal = JS_NewCFunction2(ctx, (JSCFunction *)func, name, length, cproto, magic);
    JS_FreeCString(ctx, name);
    return funcVal;
}

JSValue JSB_NewPropertyObject(JSContext *ctx, JSValueConst this_obj, JSAtom atom, int flags)
{
    JSValue p = JS_GetProperty(ctx, this_obj, atom);
    if (JS_IsObject(p))
    {
        return p;
    }
    JS_FreeValue(ctx, p); // release old value
    p = JS_NewObject(ctx);
    JS_DupValue(ctx, p);
    JS_DefinePropertyValue(ctx, this_obj, atom, p, flags);
    return p;
}

JSValue JSB_NewPropertyObjectStr(JSContext *ctx, JSValueConst this_obj, const char *name, int flags)
{
    JSValue p = JS_GetPropertyStr(ctx, this_obj, name);
    if (JS_IsObject(p))
    {
        return p;
    }
    JS_FreeValue(ctx, p); // release old value
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
// #define JS_BO_STRICT_OBJECT 4

typedef struct JSPayloadHeader
{
    int32_t type_id; // JS_BO_*
    int32_t value;
} JSPayloadHeader;

static JSPayloadHeader _null_payload = {.type_id = 0, .value = 0};

typedef struct JSPayload
{
    JSPayloadHeader header;
    char data[1];
} JSPayload;

// added: v1
JS_BOOL jsb_set_payload(JSContext *ctx, JSValue obj, int32_t type_id, int32_t value, int32_t size)
{
    JSPayload *sv = (JSPayload *)js_mallocz(ctx, sizeof(JSPayloadHeader) + size);
    sv->header.type_id = type_id;
    sv->header.value = value;
    JS_SetOpaque(obj, sv);
    return TRUE;
}

JSValue jsb_construct_bridge_object(JSContext *ctx, JSValue proto, int32_t object_id)
{
    JSValue obj = JS_CallConstructor(ctx, proto, 0, NULL);
    if (!JS_IsException(obj))
    {
        JSPayload *sv = (JSPayload *)js_malloc(ctx, sizeof(JSPayloadHeader));
        sv->header.type_id = JS_BO_OBJECT;
        sv->header.value = object_id;
        JS_SetOpaque(obj, sv);
    }

    return obj;
}

JSValue jsb_new_bridge_object(JSContext *ctx, JSValue proto, int32_t object_id/*, int32_t type_id = JS_BO_OBJECT */)
{
    JSValue obj = JS_NewObjectProtoClass(ctx, proto, js_bridge_class_id);
    if (!JS_IsException(obj))
    {
        JSPayload *sv = (JSPayload *)js_malloc(ctx, sizeof(JSPayloadHeader));
        sv->header.type_id = JS_BO_OBJECT;
        sv->header.value = object_id;
        JS_SetOpaque(obj, sv);
    }

    return obj;
}

// for constructor new_target
JSValue JSB_NewBridgeClassObject(JSContext *ctx, JSValue new_target, int32_t object_id/*, int32_t type_id = JS_BO_OBJECT */)
{
    JSValue proto = JS_GetProperty(ctx, new_target, JS_ATOM_prototype);
    if (!JS_IsException(proto))
    {
        JSValue obj = jsb_new_bridge_object(ctx, proto, object_id/*, type_id*/);
        JS_FreeValue(ctx, proto);
        return obj;
    }

    return proto;
}

JSValue jsb_new_bridge_value(JSContext *ctx, JSValue proto, int32_t size)
{
    JSValue obj = JS_NewObjectProtoClass(ctx, proto, js_bridge_class_id);
    if (!JS_IsException(obj))
    {
        JSPayload *sv = (JSPayload *)js_mallocz(ctx, sizeof(JSPayloadHeader) + size);
        sv->header.type_id = JS_BO_VALUE;
        sv->header.value = size;
        JS_SetOpaque(obj, sv);
    }

    return obj;
}

JSValue JSB_NewBridgeClassValue(JSContext *ctx, JSValue new_target, int32_t size)
{
    JSValue proto = JS_GetProperty(ctx, new_target, JS_ATOM_prototype);
    if (!JS_IsException(proto))
    {
        JSValue obj = jsb_new_bridge_value(ctx, proto, size);
        JS_FreeValue(ctx, proto);
        return obj;
    }

    return proto;
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

JS_BOOL jsb_get_floats(JSValue val, int n, float *v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * n)
    {
        float *ptr = (float *)&(sv->data[0]);
        for (int i = 0; i < n; ++i)
        {
            *(v0 + i) = ptr[i];
        }
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_floats(JSValue val, int n, float *v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * n)
    {
        float *ptr = (float *)&(sv->data[0]);
        for (int i = 0; i < n; ++i)
        {
            ptr[i] = *(v0 + i);
        }
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_float_2(JSValue val, float *v0, float *v1)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * 2)
    {
        float *ptr = (float *)&(sv->data[0]);
        *v0 = ptr[0];
        *v1 = ptr[1];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_float_2(JSValue val, float v0, float v1)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * 2)
    {
        float *ptr = (float *)&(sv->data[0]);
        ptr[0] = v0;
        ptr[1] = v1;
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_float_3(JSValue val, float *v0, float *v1, float *v2)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * 3)
    {
        float *ptr = (float *)&(sv->data[0]);
        *v0 = ptr[0];
        *v1 = ptr[1];
        *v2 = ptr[2];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_float_3(JSValue val, float v0, float v1, float v2)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * 3)
    {
        float *ptr = (float *)&(sv->data[0]);
        ptr[0] = v0;
        ptr[1] = v1;
        ptr[2] = v2;
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_float_4(JSValue val, float *v0, float *v1, float *v2, float *v3)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * 4)
    {
        float *ptr = (float *)&(sv->data[0]);
        *v0 = ptr[0];
        *v1 = ptr[1];
        *v2 = ptr[2];
        *v3 = ptr[3];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_float_4(JSValue val, float v0, float v1, float v2, float v3)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(float) * 4)
    {
        float *ptr = (float *)&(sv->data[0]);
        ptr[0] = v0;
        ptr[1] = v1;
        ptr[2] = v2;
        ptr[3] = v3;
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_ints(JSValue val, int n, int *v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * n)
    {
        int *ptr = (int *)&(sv->data[0]);
        for (int i = 0; i < n; ++i)
        {
            *(v0 + i) = ptr[i];
        }
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_ints(JSValue val, int n, int *v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * n)
    {
        int *ptr = (int *)&(sv->data[0]);
        for (int i = 0; i < n; ++i)
        {
            ptr[i] = *(v0 + i);
        }
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_int_1(JSValue val, int *v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int))
    {
        int *ptr = (int *)&(sv->data[0]);
        *v0 = ptr[0];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_int_1(JSValue val, int v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int))
    {
        int *ptr = (int *)&(sv->data[0]);
        ptr[0] = v0;
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_int_2(JSValue val, int *v0, int *v1)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * 2)
    {
        int *ptr = (int *)&(sv->data[0]);
        *v0 = ptr[0];
        *v1 = ptr[1];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_int_2(JSValue val, int v0, int v1)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * 2)
    {
        int *ptr = (int *)&(sv->data[0]);
        ptr[0] = v0;
        ptr[1] = v1;
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_int_3(JSValue val, int *v0, int *v1, int *v2)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * 3)
    {
        int *ptr = (int *)&(sv->data[0]);
        *v0 = ptr[0];
        *v1 = ptr[1];
        *v2 = ptr[2];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_int_3(JSValue val, int v0, int v1, int v2)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * 3)
    {
        int *ptr = (int *)&(sv->data[0]);
        ptr[0] = v0;
        ptr[1] = v1;
        ptr[2] = v2;
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_int_4(JSValue val, int *v0, int *v1, int *v2, int *v3)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * 4)
    {
        int *ptr = (int *)&(sv->data[0]);
        *v0 = ptr[0];
        *v1 = ptr[1];
        *v2 = ptr[2];
        *v3 = ptr[3];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_int_4(JSValue val, int v0, int v1, int v2, int v3)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(int) * 4)
    {
        int *ptr = (int *)&(sv->data[0]);
        ptr[0] = v0;
        ptr[1] = v1;
        ptr[2] = v2;
        ptr[3] = v3;
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_bytes(JSValue val, int n, byte *v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(byte) * n)
    {
        byte *ptr = (byte *)&(sv->data[0]);
        for (int i = 0; i < n; ++i)
        {
            *(v0 + i) = ptr[i];
        }
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_bytes(JSValue val, int n, byte *v0)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(byte) * n)
    {
        byte *ptr = (byte *)&(sv->data[0]);
        for (int i = 0; i < n; ++i)
        {
            ptr[i] = *(v0 + i);
        }
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_get_byte_4(JSValue val, byte *v0, byte *v1, byte *v2, byte *v3)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(byte) * 4)
    {
        byte *ptr = (byte *)&(sv->data[0]);
        *v0 = ptr[0];
        *v1 = ptr[1];
        *v2 = ptr[2];
        *v3 = ptr[3];
        return TRUE;
    }
    return FALSE;
}

JS_BOOL jsb_set_byte_4(JSValue val, byte v0, byte v1, byte v2, byte v3)
{
    JSPayload *sv = JS_GetOpaque(val, js_bridge_class_id);
    if (sv && sv->header.type_id == JS_BO_VALUE && sv->header.value == sizeof(byte) * 4)
    {
        byte *ptr = (byte *)&(sv->data[0]);
        ptr[0] = v0;
        ptr[1] = v1;
        ptr[2] = v2;
        ptr[3] = v3;
        return TRUE;
    }
    return FALSE;
}

int JSB_Init()
{
    if (js_class_id_begin == 0)
    {
        JS_NewClassID(&js_bridge_class_id);
        JS_NewClassID(&js_class_id_begin);
    }

    // 2020-12-02
    return 0xa; // version tag for unity_qjs.c
}
