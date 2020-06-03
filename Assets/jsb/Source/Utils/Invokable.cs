using System;

namespace QuickJS.Utils
{
    public interface Invokable : IDisposable
    {
        void Invoke();
    }

    public class InvokableAction : Invokable
    {
        private Action _fn;

        public InvokableAction(Action fn)
        {
            _fn = fn;
        }

        public void Invoke()
        {
            try
            {
                _fn();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError(exception);
            }
        }

        public void Dispose()
        {
            _fn = null;
        }
    }
}