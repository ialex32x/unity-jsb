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
        private bool _isVarargMethod;

        public DynamicMethod(DynamicType type, MethodInfo methodInfo)
        {
            _type = type;
            _methodInfo = methodInfo;
            _methodParameters = _methodInfo.GetParameters();
            _isVarargMethod = Values.IsVarargParameter(_methodParameters);

            var argIndex = 0;
            for (var i = 0; i < _methodParameters.Length; i++)
            {
                var p = _methodParameters[i];
                if (!Values.IsContextualType(p.ParameterType))
                {
                    argIndex++;
                }
            }
            _inputParameters = new ParameterInfo[argIndex];
            argIndex = 0;
            for (var i = 0; i < _methodParameters.Length; i++)
            {
                var p = _methodParameters[i];
                if (!Values.IsContextualType(p.ParameterType))
                {
                    _inputParameters[argIndex++] = p;
                }
            }
        }

        public override ParameterInfo[] GetParameters()
        {
            return _inputParameters;
        }

        public override bool CheckArgs(JSContext ctx, int argc, JSValue[] argv)
        {
            if (_isVarargMethod)
            {
                if (_inputParameters.Length - 1 > argc)
                {
                    return false;
                }

                return Values.js_match_parameters_vararg(ctx, argv, _inputParameters);
            }
            else
            {
                if (_inputParameters.Length != argc)
                {
                    return false;
                }

                return Values.js_match_parameters(ctx, argv, _inputParameters);
            }
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
                if (!Values.js_get_var(ctx, this_obj, _type.type, out self) || !_type.CheckThis(self))
                {
                    throw new ThisBoundException();
                }
            }
            var nArgs = _methodParameters.Length;
            var args = new object[nArgs];
            var vIndex = 0;
            var bBackValues = false;
            for (var i = 0; i < nArgs; i++)
            {
                var parameterInfo = _methodParameters[i];
                var pType = parameterInfo.ParameterType;
                if (Values.IsContextualType(pType))
                {
                    args[i] = Values.js_get_context(ctx, pType);
                }
                else
                {
                    if (_isVarargMethod && i == nArgs - 1)
                    {
                        var varArgLength = argc - nArgs + 1;
                        var varArgType = pType.GetElementType();
                        var varArgArray = Array.CreateInstance(varArgType, varArgLength);
                        for (var varArgIndex = 0; varArgIndex < varArgLength; varArgIndex++)
                        {
                            object varArgElement = null;
                            if (!Values.js_get_var(ctx, argv[vIndex++], varArgType, out varArgElement))
                            {
                                return JSApi.JS_ThrowInternalError(ctx, $"failed to cast val (vararg {varArgIndex})");
                            }
                            varArgArray.SetValue(varArgElement, varArgIndex);
                        }
                        args[i] = varArgArray;
                    }
                    else
                    {
                        if (pType.IsByRef)
                        {
                            bBackValues = true;
                            if (!parameterInfo.IsOut)
                            {
                                if (!Values.js_get_var(ctx, argv[vIndex], pType.GetElementType(), out args[i]))
                                {
                                    return JSApi.JS_ThrowInternalError(ctx, $"failed to cast val {vIndex}");
                                }
                            }
                        }
                        else
                        {
                            if (!Values.js_get_var(ctx, argv[vIndex], pType, out args[i]))
                            {
                                return JSApi.JS_ThrowInternalError(ctx, $"failed to cast val {vIndex}");
                            }
                        }
                        vIndex++;
                    }
                }
            }

            var ret = _methodInfo.Invoke(self, args);

            if (bBackValues)
            {
                vIndex = 0;
                for (var i = 0; i < nArgs; i++)
                {
                    var parameterInfo = _methodParameters[i];
                    var pType = parameterInfo.ParameterType;
                    if (!Values.IsContextualType(pType))
                    {
                        if (_isVarargMethod && i == nArgs - 1)
                        {
                        }
                        else
                        {
                            if (pType.IsByRef)
                            {
                                var backValue = Values.js_push_var(ctx, args[i]);
                                var valueAtom = ScriptEngine.GetContext(ctx).GetAtom("value");
                                JSApi.JS_SetProperty(ctx, argv[vIndex], valueAtom, backValue);
                            }

                            vIndex++;
                        }
                    }
                }
            }

            if (_type.type.IsValueType && !_methodInfo.IsStatic)
            {
                Values.js_rebind_var(ctx, this_obj, _type.type, self);
            }

            if (_methodInfo.ReturnType != typeof(void))
            {
                return Values.js_push_var(ctx, ret);
            }

            return JSApi.JS_UNDEFINED;
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
            var ret = methodInfo.Invoke(self, args);

            if (methodInfo.ReturnType != typeof(void))
            {
                return Values.js_push_var(ctx, ret);
            }

            return JSApi.JS_UNDEFINED;
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
        private ParameterInfo[] _parameters;
        private bool _disposable;

        public DynamicConstructor(DynamicType type, ConstructorInfo ctor)
        : this(type, ctor, false)
        {
        }

        public DynamicConstructor(DynamicType type, ConstructorInfo ctor, bool disposable)
        {
            _type = type;
            _ctor = ctor;
            _parameters = _ctor.GetParameters();
            _disposable = disposable;
        }

        public override ParameterInfo[] GetParameters()
        {
            return _parameters;
        }

        public override bool CheckArgs(JSContext ctx, int argc, JSValue[] argv)
        {
            if (_parameters.Length != argc)
            {
                return false;
            }

            return Values.js_match_parameters(ctx, argv, _parameters);
        }

        public override JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (!_ctor.IsPublic && !_type.privateAccess)
            {
                return JSApi.JS_ThrowInternalError(ctx, "constructor is inaccessible due to its protection level");
            }

            var nArgs = argc;
            var args = new object[nArgs];
            for (var i = 0; i < nArgs; i++)
            {
                if (!Values.js_get_var(ctx, argv[i], _parameters[i].ParameterType, out args[i]))
                {
                    return JSApi.JS_ThrowInternalError(ctx, "failed to cast val");
                }
            }

            var inst = _ctor.Invoke(args);
            var val = Values.js_new_var(ctx, this_obj, _type.type, inst, _type.id, _disposable);
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
