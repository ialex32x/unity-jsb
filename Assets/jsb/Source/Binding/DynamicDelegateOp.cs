using System;
using System.Reflection;
using QuickJS.Native;

namespace QuickJS.Binding
{
    // 委托 add/remove/set/get 操作包装函数
    public class DynamicFieldDelegateOp : IDynamicMethod
    {
        private string _varName;
        private DynamicType _type;
        private FieldInfo _fieldInfo;

        public DynamicFieldDelegateOp(DynamicType type, FieldInfo fieldInfo, string varName)
        {
            _type = type;
            _fieldInfo = fieldInfo;
            _varName = string.IsNullOrEmpty(varName) ? _fieldInfo.Name : varName;
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
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
        private string _varName;
        private DynamicType _type;
        private PropertyInfo _propertyInfo;

        public DynamicPropertyDelegateOp(DynamicType type, PropertyInfo propertyInfo, string varName)
        {
            _type = type;
            _propertyInfo = propertyInfo;
            _varName = string.IsNullOrEmpty(varName) ? _propertyInfo.Name : varName;
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
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

    public class DynamicEventDelegateOp : IDynamicMethod
    {
        private string _varName;
        private DynamicType _type;
        private EventInfo _eventInfo;

        public DynamicEventDelegateOp(DynamicType type, EventInfo eventInfo, string varName)
        {
            _type = type;
            _eventInfo = eventInfo;
            _varName = string.IsNullOrEmpty(varName) ? _eventInfo.Name : varName;
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                var op = Values.js_parse_event_op(ctx, argv[0]);
                var delegateType = _eventInfo.EventHandlerType;

                switch (op)
                {
                    case Values.EVT_OP_ADD:
                        {
                            if (_eventInfo.AddMethod == null)
                            {
                                throw new InaccessibleMemberException(_eventInfo.Name);
                            }

                            if (!_eventInfo.AddMethod.IsPublic && !_type.privateAccess)
                            {
                                throw new InaccessibleMemberException(_eventInfo.Name);
                            }

                            object self = null;
                            if (!_eventInfo.AddMethod.IsStatic)
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
                            }
                            _eventInfo.AddEventHandler(self, value);
                            return JSApi.JS_UNDEFINED;
                        }
                    case Values.EVT_OP_REMOVE:
                        {
                            if (_eventInfo.RemoveMethod == null)
                            {
                                throw new InaccessibleMemberException(_eventInfo.Name);
                            }

                            if (!_eventInfo.RemoveMethod.IsPublic && !_type.privateAccess)
                            {
                                throw new InaccessibleMemberException(_eventInfo.Name);
                            }

                            object self = null;
                            if (!_eventInfo.RemoveMethod.IsStatic)
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
                                throw new ParameterException(_type.type, _varName, delegateType, 1);
                            }
                            _eventInfo.RemoveEventHandler(self, value);
                            return JSApi.JS_UNDEFINED;
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