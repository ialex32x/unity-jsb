using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    public class UnityBindingLogger : IBindingLogger
    {
        public void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        public void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
