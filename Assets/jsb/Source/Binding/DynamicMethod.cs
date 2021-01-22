using System;
using System.Reflection;
using QuickJS.Native;

namespace QuickJS.Binding
{
    public interface IDynamicMethod
    {
        JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv);
    }

    public abstract class DynamicMethodBase : IDynamicMethod
    {
        public abstract ParameterInfo[] GetParameters();

        public abstract bool CheckArgs(JSContext ctx, int argc, JSValue[] argv);

        public abstract JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv);
    }

    public class DynamicMethod : DynamicMethodBase
    {
        private DynamicType _type;
        private MethodInfo _methodInfo;

        private ParameterInfo[] _inputParameters;
        private ParameterInfo[] _methodParameters;

        protected Values.CSValueCast _rvalPusher;

        public DynamicMethod(DynamicType type, MethodInfo methodInfo)
        {
            _type = type;
            _methodInfo = methodInfo;
            _methodParameters = _methodInfo.GetParameters();

            var argIndex = 0;
            for (var i = 0; i < _methodParameters.Length; i++)
            {
                var p = _methodParameters[i];
                if (!Values.IsAutoBindArgType(p.ParameterType))
                {
                    argIndex++;
                }
            }
            _inputParameters = new ParameterInfo[argIndex];
            argIndex = 0;
            for (var i = 0; i < _methodParameters.Length; i++)
            {
                var p = _methodParameters[i];
                if (!Values.IsAutoBindArgType(p.ParameterType))
                {
                    _inputParameters[argIndex++] = p;
                }
            }
        }

        public void ReplaceRValPusher(Values.CSValueCast rvalPusher)
        {
            _rvalPusher = rvalPusher;
        }

        public override ParameterInfo[] GetParameters()
        {
            return _inputParameters;
        }

        public override bool CheckArgs(JSContext ctx, int argc, JSValue[] argv)
        {
            if (_inputParameters.Length != argc)
            {
                return false;
            }

            return Values.js_match_parameters(ctx, argv, _inputParameters);
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (!_methodInfo.IsPublic && !_type.privateAccess)
            {
                return JSApi.JS_ThrowInternalError(ctx, "method is inaccessible due to its protection level");
            }
            object self = null;
            if (!_methodInfo.IsStatic)
            {
                Values.js_get_cached_object(ctx, this_obj, out self);
                if (!_type.CheckThis(self))
                {
                    throw new ThisBoundException();
                }
            }
            var nArgs = _methodParameters.Length;
            var args = new object[nArgs];
            var vIndex = 0;
            for (var i = 0; i < nArgs; i++)
            {
                var pType = _methodParameters[i].ParameterType;
                if (Values.IsAutoBindArgType(pType))
                {
                    args[i] = Values.js_get_context(ctx, pType);
                }
                else
                {
                    if (!Values.js_get_var(ctx, argv[vIndex++], pType, out args[i]))
                    {
                        return JSApi.JS_ThrowInternalError(ctx, "failed to cast val");
                    }
                }
            }

            if (_methodInfo.ReturnType != typeof(void))
            {
                var ret = _methodInfo.Invoke(self, args);

                if (_rvalPusher != null)
                {
                    return _rvalPusher(ctx, ret);
                }
                return Values.js_push_var(ctx, ret);
            }
            else
            {
                _methodInfo.Invoke(self, args);
                return JSApi.JS_UNDEFINED;
            }
        }
    }

    public class DynamicDelegateMethod : IDynamicMethod
    {
        private Delegate _delegate;

        public DynamicDelegateMethod(Delegate d)
        {
            _delegate = d;
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            var self = _delegate.Target;
            var methodInfo = _delegate.Method;
            var parameters = methodInfo.GetParameters();
            var nArgs = argc;
            var args = new object[nArgs];
            for (var i = 0; i < nArgs; i++)
            {
                if (!Values.js_get_var(ctx, argv[i], parameters[i].ParameterType, out args[i]))
                {
                    return JSApi.JS_ThrowInternalError(ctx, "failed to cast val");
                }
            }
            if (methodInfo.ReturnType != typeof(void))
            {
                var ret = methodInfo.Invoke(self, args);
                return Values.js_push_var(ctx, ret);
            }
            else
            {
                methodInfo.Invoke(self, args);
                return JSApi.JS_UNDEFINED;
            }
        }
    }

    public class DynamicCrossBindConstructor : DynamicMethodBase
    {
        public DynamicCrossBindConstructor()
        {
        }

        public override ParameterInfo[] GetParameters()
        {
            return new ParameterInfo[0];
        }

        public override bool CheckArgs(JSContext ctx, int argc, JSValue[] argv)
        {
            return true;
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            return Values._js_crossbind_constructor(ctx, this_obj);
        }
    }

    public class DynamicDefaultConstructor : DynamicMethodBase
    {
        private DynamicType _type;

        public DynamicDefaultConstructor(DynamicType type)
        {
            _type = type;
        }

        public override ParameterInfo[] GetParameters()
        {
            return new ParameterInfo[0];
        }

        public override bool CheckArgs(JSContext ctx, int argc, JSValue[] argv)
        {
            return true;
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            var inst = Activator.CreateInstance(_type.type);
            var val = Values.NewBridgeClassObject(ctx, this_obj, inst, _type.id, false);
            return val;
        }
    }

    public class DynamicConstructor : DynamicMethodBase
    {
        private DynamicType _type;
        private ConstructorInfo _ctor;

        public DynamicConstructor(DynamicType type, ConstructorInfo ctor)
        {
            _type = type;
            _ctor = ctor;
        }

        public override ParameterInfo[] GetParameters()
        {
            return _ctor.GetParameters();
        }

        public override bool CheckArgs(JSContext ctx, int argc, JSValue[] argv)
        {
            var parameters = _ctor.GetParameters();
            if (parameters.Length != argc)
            {
                return false;
            }

            return Values.js_match_parameters(ctx, argv, parameters);
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (!_ctor.IsPublic && !_type.privateAccess)
            {
                return JSApi.JS_ThrowInternalError(ctx, "constructor is inaccessible due to its protection level");
            }

            var parameters = _ctor.GetParameters();
            var nArgs = argc;
            var args = new object[nArgs];
            for (var i = 0; i < nArgs; i++)
            {
                if (!Values.js_get_var(ctx, argv[i], parameters[i].ParameterType, out args[i]))
                {
                    return JSApi.JS_ThrowInternalError(ctx, "failed to cast val");
                }
            }

            var inst = _ctor.Invoke(args);
            var val = Values.NewBridgeClassObject(ctx, this_obj, inst, _type.id, false);
            return val;
        }
    }

    public class DynamicMethodInvoke : IDynamicMethod
    {
        private JSCFunction _method;

        public DynamicMethodInvoke(JSCFunction method)
        {
            _method = method;
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            return _method.Invoke(ctx, this_obj, argc, argv);
        }
    }
}
