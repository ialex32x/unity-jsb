using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using Native;

    public class JSStringCache
    {
        public class Slot
        {
            public int next;
            // public JSValue jsValue;
            public string stringValue;
        }

        private bool _disposed;

        // js string => slot id
        private Dictionary<JSValue, int> _jsvMap = new Dictionary<JSValue, int>();
        // c# string => slot id
        private Dictionary<string, int> _strMap = new Dictionary<string, int>();

        private int _freeIndex = -1;
        private List<Slot> _slots = new List<Slot>();

        public void Destroy()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _strMap.Clear();
            _jsvMap.Clear();
            _slots.Clear();
        }

        public void Clear()
        {
        }

        // public int Add(string val)
        // {
        //     return Add(JSApi.JS_NewString(ctx, val));
        // }

        // public bool TryGetValue(JSValue val, out string str)
        // {
        // }

        public int AddValue(JSValue val)
        {
            // if (!_disposed && o != null)
            // {
            //     if (_freeIndex < 0)
            //     {
            //         var freeEntry = new Slot();
            //         var id = _map.Count;
            //         _map.Add(freeEntry);
            //         freeEntry.next = -1;
            //         freeEntry.stringValue = o;
            //         return id;
            //     }
            //     else
            //     {
            //         var id = _freeIndex;
            //         var freeEntry = _map[id];
            //         _freeIndex = freeEntry.next;
            //         freeEntry.next = -1;
            //         freeEntry.target = o;
            //         return id;
            //     }
            // }
            return -1;
        }
    }
}
