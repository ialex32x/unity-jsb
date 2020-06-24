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

        public virtual bool CheckArgs(int argc, JSValue[] argv)
        {
            //TODO: check args if any overload func exists
            return true;
        }

        public abstract JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv);
    }

    public class DynamicMethod : DynamicMethodBase
    {
        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            //TODO: dynamic method impl
            return JSApi.JS_UNDEFINED;
        }
    }

    public class DynamicConstructor : DynamicMethodBase
    {
        private int _type_id;
        private ConstructorInfo _ctor;

        public DynamicConstructor(ConstructorInfo ctor, int type_id)
        {
            _type_id = type_id;
            _ctor = ctor;
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                //TODO: dynamic constructor impl
                var o = _ctor.Invoke(null);
                var val = Values.NewBridgeClassObject(ctx, this_obj, o, _type_id);
                return val;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }
    }
}
