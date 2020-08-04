using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS.Extra
{
    using AOT;
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;
    using System.Threading;

    public class JSWorker
    {
        private bool _running = true;
        private Thread _thread;

        private JSWorker()
        {
        }

        private void Start(ScriptRuntime parent)
        {
            var runtime = parent.CreateWorker();

            if (runtime == null)
            {
                throw new NullReferenceException();
            }

            _thread = new Thread(new ThreadStart(Run));
            _thread.Priority = ThreadPriority.Lowest;
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            while (_running)
            {

            }

            // runtime.Destroy();
        }

        public static void Bind(TypeRegister register)
        {

        }
    }
}
