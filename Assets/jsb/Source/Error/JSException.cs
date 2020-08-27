using System;

namespace QuickJS
{
    public class JSException : Exception
    {
        public JSException(string message)
        : base(message)
        {
        }
    }
}
