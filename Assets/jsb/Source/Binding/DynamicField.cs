using System;
using System.Reflection;
using QuickJS.Native;

namespace QuickJS.Binding
{
    public interface IDynamicField
    {
        JSValue GetValue(JSContext ctx, JSValue this_val);
        JSValue SetValue(JSContext ctx, JSValue this_val, JSValue val);
    }

    public class DynamicField : IDynamicField
    {
        private DynamicType _type;
        private FieldInfo _fieldInfo;

        public DynamicField(DynamicType type, FieldInfo fieldInfo)
        {
            _type = type;
            _fieldInfo = fieldInfo;
        }

        public JSValue GetValue(JSContext ctx, JSValue this_obj)
        {
            if (!_fieldInfo.IsPublic && !_type.privateAccess)
            {
                throw new InaccessibleMemberException(_fieldInfo.Name);
            }
            object self = null;
            if (!_fieldInfo.IsStatic)
            {
                if (!Values.js_get_var(ctx, this_obj, _type.type, out self) || !_type.CheckThis(self))
                {
                    throw new ThisBoundException();
                }
            }
            
            var rval = _fieldInfo.GetValue(self);
            return Values.js_push_var(ctx, rval);
        }

        public JSValue SetValue(JSContext ctx, JSValue this_obj, JSValue val)
        {
            if (!_fieldInfo.IsPublic && !_type.privateAccess)
            {
                throw new InaccessibleMemberException(_fieldInfo.Name);
            }
            object self = null;
            if (!_fieldInfo.IsStatic)
            {
                if (!Values.js_get_var(ctx, this_obj, _type.type, out self) || !_type.CheckThis(self))
                {
                    throw new ThisBoundException();
                }
            }
            object t_val = null;
            if (!Values.js_get_var(ctx, val, _fieldInfo.FieldType, out t_val))
            {
                throw new InvalidCastException();
            }
            _fieldInfo.SetValue(self, t_val);
            
            if (_type.type.IsValueType && !_fieldInfo.IsStatic)
            {
                Values.js_rebind_var(ctx, this_obj, _type.type, self);
            }

            return JSApi.JS_UNDEFINED;
        }
    }

    public class DynamicProperty : IDynamicField
    {
        private DynamicType _type;
        private PropertyInfo _propertyInfo;

        public DynamicProperty(DynamicType type, PropertyInfo propertyInfo)
        {
            _type = type;
            _propertyInfo = propertyInfo;
        }

        public JSValue GetValue(JSContext ctx, JSValue this_obj)
        {
            if (_propertyInfo.GetMethod == null)
            {
                throw new NullReferenceException("property getter is null");
            }
            if (!_propertyInfo.GetMethod.IsPublic && !_type.privateAccess)
            {
                throw new InaccessibleMemberException(_propertyInfo.Name);
            }
            object self = null;
            if (!_propertyInfo.GetMethod.IsStatic)
            {
                if (!Values.js_get_var(ctx, this_obj, _type.type, out self) || !_type.CheckThis(self))
                {
                    throw new ThisBoundException();
                }
            }

            var rval = _propertyInfo.GetValue(self);
            return Values.js_push_var(ctx, rval);
        }

        public JSValue SetValue(JSContext ctx, JSValue this_obj, JSValue val)
        {
            if (_propertyInfo.SetMethod == null)
            {
                throw new NullReferenceException("property setter is null");
            }
            if (!_propertyInfo.SetMethod.IsPublic && !_type.privateAccess)
            {
                throw new InaccessibleMemberException(_propertyInfo.Name);
            }
            object self = null;
            if (!_propertyInfo.SetMethod.IsStatic)
            {
                if (!Values.js_get_var(ctx, this_obj, _type.type, out self) || !_type.CheckThis(self))
                {
                    throw new ThisBoundException();
                }
            }
            object t_val = null;
            if (!Values.js_get_var(ctx, val, _propertyInfo.PropertyType, out t_val))
            {
                throw new InvalidCastException();
            }
            _propertyInfo.SetValue(self, t_val);
            if (_type.type.IsValueType && !_propertyInfo.SetMethod.IsStatic)
            {
                Values.js_rebind_var(ctx, this_obj, _type.type, self);
            }
            return JSApi.JS_UNDEFINED;
        }
    }
}
