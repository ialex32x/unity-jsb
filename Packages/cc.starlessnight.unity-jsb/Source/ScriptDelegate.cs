using System;
using System.Collections.Generic;
using QuickJS.Native;

namespace QuickJS
{
    // 刻意与 ScriptValue 隔离
    public class ScriptDelegate : Utils.IWeakMapEntry
    {
        protected ScriptContext _context;
        protected /*readonly*/ JSValue _jsValue;
#if JSB_DEBUG
        protected string _stackTrack;
#endif

        // 一个 JSValue (function) 可能会被用于映射多个委托对象
        // it's safe for cycle references between managed objects without weakreference
        private List<Delegate> _matches = new List<Delegate>();

        /// <summary>
        /// 获取委托包装所在的 JSContext. 在已经释放的 ScriptDelegate 上访问此属性会抛出 NullReferenceException.
        /// </summary>
        public JSContext ctx
        {
            get { return _context; }
        }

        public bool isValid => _context != null;

        public ScriptDelegate(ScriptContext context, JSValue jsValue)
        {
            _context = context;
            _jsValue = jsValue;
            JSApi.JS_DupValue(context, jsValue);
            // ScriptDelegate 拥有 js 对象的强引用, 此 js 对象无法释放 cache 中的 object, 所以这里用弱引用注册
            // 会出现的问题是, 如果 c# 没有对 ScriptDelegate 的强引用, 那么反复 get_delegate 会重复创建 ScriptDelegate
            _context.GetObjectCache().AddDelegate(_jsValue, this);
#if JSB_DEBUG
            try { throw new Exception(); }
            catch (Exception exception) { _stackTrack = exception.StackTrace; }
#endif
        }

        public static implicit operator JSValue(ScriptDelegate value)
        {
            return value._jsValue;
        }

        ~ScriptDelegate()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool bManaged)
        {
            if (_context != null)
            {
                var context = _context;

                _context = null;
#if JSB_DEBUG
                context.GetRuntime().FreeDelegationValue(_jsValue, _stackTrack);
#else
                context.GetRuntime().FreeDelegationValue(_jsValue);
#endif
                _jsValue = JSApi.JS_UNDEFINED;
            }
        }

        public override int GetHashCode()
        {
            return _jsValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptDelegate)
            {
                var other = (ScriptDelegate)obj;
                return other._jsValue == _jsValue;
            }

            if (obj is JSValue)
            {
                var other = (JSValue)obj;
                return other == _jsValue;
            }

            return false;
        }

        public Delegate Any()
        {
            return _matches.Count != 0 ? _matches[0] : null;
        }

        public Delegate Match(Type delegateType)
        {
            for (int i = 0, count = _matches.Count; i < count; i++)
            {
                var d = _matches[i];
                if (d.GetType() == delegateType)
                {
                    return d;
                }
            }
            return null;
        }

        public void Add(Delegate d)
        {
            if (d == null)
            {
                throw new ArgumentNullException();
            }
            _matches.Add(d);
        }

        public unsafe JSValue Invoke(JSContext ctx)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, JSApi.JS_UNDEFINED, 0, (JSValue*)0);
            return rval;
        }

        public unsafe JSValue Invoke(JSContext ctx, int argc, JSValue[] argv)
        {
            fixed (JSValue* ptr = argv)
            {
                JSValue rval = JSApi.JS_Call(ctx, _jsValue, JSApi.JS_UNDEFINED, argc, ptr);
                return rval;
            }
        }

        public unsafe JSValue Invoke(JSContext ctx, int argc, JSValue* argv)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, JSApi.JS_UNDEFINED, argc, argv);
            return rval;
        }

        public unsafe JSValue Invoke(JSContext ctx, JSValue this_obj)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, this_obj, 0, (JSValue*)0);
            return rval;
        }

        public unsafe JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            fixed (JSValue* ptr = argv)
            {
                JSValue rval = JSApi.JS_Call(ctx, _jsValue, this_obj, argc, ptr);
                return rval;
            }
        }

        public unsafe JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue* argv)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, this_obj, argc, argv);
            return rval;
        }
    }
}