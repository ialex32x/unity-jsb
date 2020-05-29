using QuickJS.Native;

namespace QuickJS
{
    public class ScriptEngine
    {
        private uint _class_id_alloc = JSApi.__JSB_GetClassID();
        private JSRuntime _rt;

        public ScriptEngine()
        {
            _rt = JSApi.JS_NewRuntime();
        }

        public JSClassID NewClassID()
        {
            return _class_id_alloc++;
        }

        public void FreeValue(JSValue value)
        {
            JSApi.JS_FreeValueRT(_rt, value);
        }

        public ScriptContext NewContext()
        {
            var ctx = JSApi.JS_NewContext(_rt);
            return new ScriptContext(ctx);
        }

        public static implicit operator JSRuntime(ScriptEngine se)
        {
            return se._rt;
        }
    }
}
