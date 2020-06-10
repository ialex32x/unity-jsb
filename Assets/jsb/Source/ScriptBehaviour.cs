using System;
using System.Collections.Generic;
using AOT;

namespace QuickJS
{
    using Native;
    using Binding;
    using UnityEngine;

    public class ScriptBehaviour : MonoBehaviour
    {
        private ScriptContext _context;
        private JSContext _ctx;
        private JSValue _self;

        private bool _updateValid;
        private JSValue _updateFunc;

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue js_ctor(JSContext ctx, JSValue this_obj, int argc, JSValue[] args)
        {
            //TODO: not implemented
            return JSApi.JS_UNDEFINED;
        }

        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace("jsb");
            var cls = ns.CreateClass("Behaviour", typeof(ScriptBehaviour), js_ctor);
            cls.Close();
            ns.Close();
        }

        public void SetBridge(JSValue obj)
        {
            _self = obj;
            // _instance.InvokeMember("Awake");
            // if (enabled)
            // {
            //     _instance.InvokeMember("OnEnable");
            // }
        }

        void Update()
        {
            if (_updateValid)
            {
                var rval = JSApi.JS_Call(_ctx, _updateFunc, _self, 0, JSApi.EmptyValues);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        // void LateUpdate()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("LateUpdate");
        //     }
        // }

        // void Start()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("Start");
        //     }
        // }

        // void OnEnable()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("OnEnable");
        //     }
        // }

        // void OnDisable()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("OnDisable");
        //     }
        // }

        // void OnApplicationFocus()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("OnApplicationFocus");
        //     }
        // }

        // void OnApplicationPause()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("OnApplicationPause");
        //     }
        // }

        // void OnApplicationQuit()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("OnApplicationQuit");
        //     }
        // }

        // void OnDestroy()
        // {
        //     if (_instance != null)
        //     {
        //         _instance.InvokeMember("OnDestroy");
        //         _instance = null;
        //     }
        // }
    }
}