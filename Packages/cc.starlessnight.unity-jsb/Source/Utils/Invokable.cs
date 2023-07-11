using System;

namespace QuickJS.Utils
{
    public interface IInvokable : IDisposable
    {
        void Invoke();
    }
}