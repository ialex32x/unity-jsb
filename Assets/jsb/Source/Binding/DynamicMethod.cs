using System;
using System.Reflection;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;
using AOT;

namespace QuickJS.Binding
{
    public interface IDynamicMethod
    {
        JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv);
    }

    public abstract class DynamicMethodBase : IDynamicMethod
    {
        // private MethodBase _methodBase;

        public abstract ParameterInfo[] GetParameters();

        public abstract bool CheckArgs(JSContext ctx, int argc, JSValue[] argv);

        public abstract JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv);
    }

    public class DynamicMethod : DynamicMethodBase
    {
        private DynamicType _type;
        private MethodInfo _methodInfo;

        public DynamicMethod(DynamicType type, MethodInfo methodInfo)
        {
            _type = type;
            _methodInfo = methodInfo;
        }

        public override ParameterInfo[] GetParameters()
        {
            return _methodInfo.GetParameters();
        }

        public override bool CheckArgs(JSContext ctx, int argc, JSValue[] argv)
        {
            var parameters = _methodInfo.GetParameters();
            if (parameters.Length != argc)
            {
                return false;
            }

            return Values.js_match_parameters(ctx, argv, parameters);
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
            var parameters = _methodInfo.GetParameters();
            var nArgs = argc;
            var args = new object[nArgs];
            for (var i = 0; i < nArgs; i++)
            {
                if (!Values.js_get_var(ctx, argv[i], parameters[i].ParameterType, out args[i]))
                {
                    return JSApi.JS_ThrowInternalError(ctx, "failed to cast val");
                }
            }
            if (_methodInfo.ReturnType != typeof(void))
            {
                var ret = _methodInfo.Invoke(self, args);
                return Values.js_push_var(ctx, ret);
            }
            else
            {
                _methodInfo.Invoke(self, args);
                return JSApi.JS_UNDEFINED;
            }
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
            var val = Values.NewBridgeClassObject(ctx, this_obj, inst, _type.id);
            return val;
        }
    }
}
