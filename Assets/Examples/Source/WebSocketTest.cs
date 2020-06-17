using System;
using System.Collections;
using System.Collections.Generic;
using QuickJS.Utils;
using UnityEngine;
using WebSockets;

namespace Examples
{
    using size_t = QuickJS.Native.size_t;

    public class WebSocketTest : MonoBehaviour
    {
        public static int callback(lws wsi, lws_callback_reasons reason, IntPtr user, IntPtr @in, size_t len)
        {
            return 0;
        }

        // Start is called before the first frame update
        void Start()
        {
            WebSockets.WSApi.lws_context_destroy(IntPtr.Zero);
            var context = WebSockets.WSApi.ulws_create("default", callback, 1024 * 4, 1024 * 4);

            WebSockets.WSApi.lws_context_destroy(context);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
