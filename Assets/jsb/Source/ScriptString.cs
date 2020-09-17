using System;
using QuickJS.Native;

namespace QuickJS
{
    public class ScriptString : ScriptValue
    {
        private bool _cached;
        private string _string;

        public ScriptString(ScriptContext context, JSValue jsValue)
        : base(context, jsValue)
        {
        }

        public override string ToString()
        {
            if (!_cached)
            {
                _cached = true;
                _string = _context != null ? JSApi.GetString(_context, _jsValue) : null;
            }
            return _string;
        }
    }
}
