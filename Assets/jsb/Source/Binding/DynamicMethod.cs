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

        public abstract bool CheckArgs(int argc, JSValue[] argv);

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
        
        public override bool CheckArgs(int argc, JSValue[] argv)
        {
            //TODO: check args if any overload func exists
            // _methodInfo.GetParameters();
            return true;
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            //TODO: dynamic method impl
            return JSApi.JS_UNDEFINED;
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

        public override bool CheckArgs(int argc, JSValue[] argv)
        {
            //TODO: check args if any overload func exists
            return true;
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                //TODO: dynamic constructor impl
                var o = _ctor.Invoke(null);
                var val = Values.NewBridgeClassObject(ctx, this_obj, o, _type.id);
                return val;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }
    }
}
