using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    public class DefaultBindingLogger : IBindingLogger
    {
        public void Log(string message)
        {
            Console.WriteLine("[INFO ] {0}", message);
        }
        
        public void LogWarning(string message)
        {
            Console.WriteLine("[WARN ] {0}", message);
        }
        
        public void LogError(string message)
        {
            Console.WriteLine("[ERROR] {0}", message);
        }
    }
}
