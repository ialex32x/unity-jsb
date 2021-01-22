using System;
using System.Reflection;
using QuickJS.Native;

namespace QuickJS.Binding
{
    // 委托 add/remove/set/get 操作包装函数
    public class DynamicFieldDelegateOp : IDynamicMethod
    {
        private DynamicType _type;
        private FieldInfo _fieldInfo;

        public DynamicFieldDelegateOp(DynamicType type, FieldInfo fieldInfo)
        {
            _type = type;
            _fieldInfo = fieldInfo;
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (!_fieldInfo.IsPublic && !_type.privateAccess)
            {
                throw new InaccessibleMemberException(_fieldInfo.Name);
            }

            object self = null;
            if (!_fieldInfo.IsStatic)
            {
                Values.js_get_cached_object(ctx, this_obj, out self);
                if (!_type.CheckThis(self))
                {
                    throw new ThisBoundException();
                }
            }

            try
            {
                var op = Values.js_parse_event_op(ctx, argv[0]);
                var delegateType = _fieldInfo.FieldType;

                switch (op)
                {
                    case Values.EVT_OP_ADD:
                        {
                            Delegate value;
                            if (!Values.js_get_delegate(ctx, argv[1], delegateType, out value))
                            {
                                throw new ParameterException(typeof(Example.DelegateTest), "onActionWithArgs", delegateType, 1);
                            }
                            var fValue = (Delegate)_fieldInfo.GetValue(self);
                            _fieldInfo.SetValue(self, Delegate.Combine(fValue, value));
                            return JSApi.JS_UNDEFINED;
                        }
                    case Values.EVT_OP_REMOVE:
                        {
                            Delegate value;
                            if (!Values.js_get_delegate(ctx, argv[1], delegateType, out value))
                            {
                                throw new ParameterException(typeof(Example.DelegateTest), "onActionWithArgs", delegateType, 1);
                            }
                            var fValue = (Delegate)_fieldInfo.GetValue(self);
                            _fieldInfo.SetValue(self, Delegate.Remove(fValue, value));
                            return JSApi.JS_UNDEFINED;
                        }
                    case Values.EVT_OP_SET:
                        {
                            Delegate value;
                            if (!Values.js_get_delegate(ctx, argv[1], delegateType, out value))
                            {
                                throw new ParameterException(typeof(Example.DelegateTest), "onActionWithArgs", delegateType, 1);
                            }
                            _fieldInfo.SetValue(self, value);
                            return JSApi.JS_UNDEFINED;
                        }
                    case Values.EVT_OP_GET:
                        {
                            var ret = (Delegate)_fieldInfo.GetValue(self);
                            return Values.js_push_delegate(ctx, ret);
                        }
                    default: throw new JSException("invalid event op");
                }
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }
    }

    public class DynamicPropertyDelegateOp : IDynamicMethod
    {
        private DynamicType _type;
        private PropertyInfo _propertyInfo;

        public DynamicPropertyDelegateOp(DynamicType type, PropertyInfo propertyInfo)
        {
            _type = type;
            _propertyInfo = propertyInfo;
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                var op = Values.js_parse_event_op(ctx, argv[0]);
                var delegateType = _propertyInfo.PropertyType;

                switch (op)
                {
                    case Values.EVT_OP_ADD:
                        {
                            if (_propertyInfo.GetMethod == null || _propertyInfo.SetMethod == null)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            if ((!_propertyInfo.GetMethod.IsPublic || !_propertyInfo.SetMethod.IsPublic) && !_type.privateAccess)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            object self = null;
                            if (!_propertyInfo.GetMethod.IsStatic)
                            {
                                Values.js_get_cached_object(ctx, this_obj, out self);
                                if (!_type.CheckThis(self))
                                {
                                    throw new ThisBoundException();
                                }
                            }

                            Delegate value;
                            if (!Values.js_get_delegate(ctx, argv[1], delegateType, out value))
                            {
                                throw new ParameterException(typeof(Example.DelegateTest), "onActionWithArgs", delegateType, 1);
                            }
                            var fValue = (Delegate)_propertyInfo.GetValue(self);
                            _propertyInfo.SetValue(self, Delegate.Combine(fValue, value));
                            return JSApi.JS_UNDEFINED;
                        }
                    case Values.EVT_OP_REMOVE:
                        {
                            if (_propertyInfo.GetMethod == null || _propertyInfo.SetMethod == null)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            if ((!_propertyInfo.GetMethod.IsPublic || !_propertyInfo.SetMethod.IsPublic) && !_type.privateAccess)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            object self = null;
                            if (!_propertyInfo.GetMethod.IsStatic)
                            {
                                Values.js_get_cached_object(ctx, this_obj, out self);
                                if (!_type.CheckThis(self))
                                {
                                    throw new ThisBoundException();
                                }
                            }

                            Delegate value;
                            if (!Values.js_get_delegate(ctx, argv[1], delegateType, out value))
                            {
                                throw new ParameterException(typeof(Example.DelegateTest), "onActionWithArgs", delegateType, 1);
                            }
                            var fValue = (Delegate)_propertyInfo.GetValue(self);
                            _propertyInfo.SetValue(self, Delegate.Remove(fValue, value));
                            return JSApi.JS_UNDEFINED;
                        }
                    case Values.EVT_OP_SET:
                        {
                            if (_propertyInfo.SetMethod == null)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            if (!_propertyInfo.SetMethod.IsPublic && !_type.privateAccess)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            object self = null;
                            if (!_propertyInfo.SetMethod.IsStatic)
                            {
                                Values.js_get_cached_object(ctx, this_obj, out self);
                                if (!_type.CheckThis(self))
                                {
                                    throw new ThisBoundException();
                                }
                            }

                            Delegate value;
                            if (!Values.js_get_delegate(ctx, argv[1], delegateType, out value))
                            {
                                throw new ParameterException(typeof(Example.DelegateTest), "onActionWithArgs", delegateType, 1);
                            }
                            _propertyInfo.SetValue(self, value);
                            return JSApi.JS_UNDEFINED;
                        }
                    case Values.EVT_OP_GET:
                        {
                            if (_propertyInfo.GetMethod == null)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            if (!_propertyInfo.GetMethod.IsPublic && !_type.privateAccess)
                            {
                                throw new InaccessibleMemberException(_propertyInfo.Name);
                            }

                            object self = null;
                            if (!_propertyInfo.GetMethod.IsStatic)
                            {
                                Values.js_get_cached_object(ctx, this_obj, out self);
                                if (!_type.CheckThis(self))
                                {
                                    throw new ThisBoundException();
                                }
                            }

                            var ret = (Delegate)_propertyInfo.GetValue(self);
                            return Values.js_push_delegate(ctx, ret);
                        }
                    default: throw new JSException("invalid event op");
                }
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }
    }
}