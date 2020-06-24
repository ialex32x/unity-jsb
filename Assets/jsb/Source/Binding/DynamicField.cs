using System;
using System.Reflection;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;
using AOT;

namespace QuickJS.Binding
{
    public interface IDynamicField
    {
        JSValue GetValue(JSContext ctx, JSValue this_val);
        JSValue SetValue(JSContext ctx, JSValue this_val, JSValue val);
    }

    public class DynamicField : IDynamicField
    {
        private FieldInfo _fieldInfo;

        public DynamicField(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }

        public JSValue GetValue(JSContext ctx, JSValue this_obj)
        {
            object self = null;
            if (!_fieldInfo.IsStatic)
            {
                Values.js_get_cached_object(ctx, this_obj, out self);
            }
            return Values.js_push_var(ctx, _fieldInfo.GetValue(self));
        }

        public JSValue SetValue(JSContext ctx, JSValue this_obj, JSValue val)
        {
            object self = null;
            if (!_fieldInfo.IsStatic)
            {
                Values.js_get_cached_object(ctx, this_obj, out self);
            }
            object t_val = null;
            if (!Values.js_get_var(ctx, val, out t_val))
            {
                return JSApi.JS_ThrowInternalError(ctx, "failed to cast val");
            }
            _fieldInfo.SetValue(self, t_val);
            return JSApi.JS_UNDEFINED;
        }
    }

    public class DynamicProperty : IDynamicField
    {
        private PropertyInfo _propertyInfo;

        public DynamicProperty(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public JSValue GetValue(JSContext ctx, JSValue this_obj)
        {
            if (_propertyInfo.GetMethod == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "property getter is null");
            }
            object self = null;
            if (!_propertyInfo.GetMethod.IsStatic)
            {
                Values.js_get_cached_object(ctx, this_obj, out self);
            }
            return Values.js_push_var(ctx, _propertyInfo.GetValue(self));
        }

        public JSValue SetValue(JSContext ctx, JSValue this_obj, JSValue val)
        {
            if (_propertyInfo.SetMethod == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "property setter is null");
            }
            object self = null;
            if (!_propertyInfo.SetMethod.IsStatic)
            {
                Values.js_get_cached_object(ctx, this_obj, out self);
            }
            object t_val = null;
            if (!Values.js_get_var(ctx, val, out t_val))
            {
                return JSApi.JS_ThrowInternalError(ctx, "failed to cast val");
            }
            _propertyInfo.SetValue(self, t_val);
            return JSApi.JS_UNDEFINED;
        }
    }
}
