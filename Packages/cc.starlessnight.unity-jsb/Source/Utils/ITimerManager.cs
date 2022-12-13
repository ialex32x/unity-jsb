using System;
using System.Collections.Generic;
using QuickJS.Binding;
using QuickJS.Native;

namespace QuickJS.Utils
{
    public interface ITimerManager : IEnumerable<TimerInfo>
    {
        #region Timer Management
        uint SetTimeout(ScriptFunction fn, int ms);
        uint SetInterval(ScriptFunction fn, int ms);
        bool ClearTimer(uint id);
        #endregion

        void Bind(TypeRegister typeRegister);

        int now { get; }

        void Update(int milliseconds);
        void Destroy();
    }
}
