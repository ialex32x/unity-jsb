using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    public interface IBindingLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
