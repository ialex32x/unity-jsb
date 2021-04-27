using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Binding
{
    public class DefaultBindingLogger : IBindingLogger
    {
        public void Log(string message)
        {
#if JSB_UNITYLESS
            Console.WriteLine("[INFO ] {0}", message);
#else
            UnityEngine.Debug.Log(message);
#endif
        }

        public void LogWarning(string message)
        {
#if JSB_UNITYLESS
            Console.WriteLine("[WARN ] {0}", message);
#else
            UnityEngine.Debug.LogWarning(message);
#endif
        }

        public void LogError(string message)
        {
#if JSB_UNITYLESS
            Console.WriteLine("[ERROR] {0}", message);
#else
            UnityEngine.Debug.LogError(message);
#endif
        }
    }
}
