using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using Native;

    public class JSStringCache
    {
        const uint kInitialStringCacheSize = 64;

        private struct Slot
        {
            public int next;
            public JSValue jsValue;
            public string stringValue;
        }

        private bool _disposed;

        // js string => slot id
        private Dictionary<JSValue, int> _jsvMap = new Dictionary<JSValue, int>();
        // c# string => slot id
        private Dictionary<string, int> _strMap = new Dictionary<string, int>();

        private int _freeIndex = -1;
        private int _allocated = 0;
        private Slot[] _slots = new Slot[kInitialStringCacheSize];

        private JSContext _ctx;

        public JSStringCache(JSContext ctx)
        {
            _ctx = ctx;
        }

        public void Destroy()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            Clear();
        }

        public int GetStringCount()
        {
            return _strMap.Count;
        }

        public void Clear()
        {
            if (_jsvMap.Count > 0)
            {
                _jsvMap.Clear();
                foreach (var kv in _strMap)
                {
                    var slotIndex = kv.Value;
                    ref var slot = ref _slots[slotIndex];
                    JSApi.JS_FreeValue(_ctx, slot.jsValue);
                    slot.jsValue = JSApi.JS_UNDEFINED;
                    slot.stringValue = null;
                }
                _strMap.Clear();
                _freeIndex = -1;
                _allocated = 0;
            }
        }

        public void RemoveValue(string o)
        {
            if (_disposed || o == null)
            {
                return;
            }

            int slotIndex;
            if (_strMap.TryGetValue(o, out slotIndex))
            {
                ref var slot = ref _slots[slotIndex];
                _strMap.Remove(o);
                _jsvMap.Remove(slot.jsValue);
                JSApi.JS_FreeValue(_ctx, slot.jsValue);
                slot.jsValue = JSApi.JS_UNDEFINED;
                slot.stringValue = null;
                slot.next = _freeIndex;
                _freeIndex = slotIndex;
            }
        }

        /// <summary>
        /// the returned jsValue is not reference added, DupValue call is required if jsValue used/stored out of the cache
        /// </summary>
        public bool AddValue(string stringValue, out JSValue jsValue)
        {
            if (_disposed || stringValue == null)
            {
                jsValue = JSApi.JS_UNDEFINED;
                return false;
            }

            jsValue = _ctx.NewString(stringValue);
            if (jsValue.IsString())
            {
                return _AddPair(jsValue, stringValue) >= 0;
            }
            JSApi.JS_FreeValue(_ctx, jsValue);
            jsValue = JSApi.JS_UNDEFINED;
            return false;
        }

        public bool AddValue(JSValue jsValue, out string stringValue)
        {
            if (_disposed || !jsValue.IsString())
            {
                stringValue = null;
                return false;
            }

            stringValue = JSApi.GetNonNullString(_ctx, jsValue);
            if (stringValue != null)
            {
                return _AddPair(JSApi.JS_DupValue(_ctx, jsValue), stringValue) >= 0;
            }
            return false;
        }

        private int _AddPair(JSValue jsValue, string stringValue)
        {
            int findSlotIndex;
            if (_strMap.TryGetValue(stringValue, out findSlotIndex))
            {
#if JSB_DEBUG
                ref var slot = ref _slots[findSlotIndex];
                Diagnostics.Assert.Debug(slot.jsValue == jsValue, "corrupted string cache: {0} != {1} => {2}", slot.jsValue, jsValue, slot.stringValue);
#endif
                return findSlotIndex;
            }

            if (_freeIndex < 0)
            {
                var oldSize = _slots.Length;
                var id = _allocated++;
                if (id >= oldSize)
                {
                    Array.Resize(ref _slots, oldSize < 8192 ? oldSize * 2 : oldSize + 1024);
                }
                ref var slot = ref _slots[id];
                slot.next = -1;
                slot.stringValue = stringValue;
                slot.jsValue = jsValue;
                _strMap[stringValue] = id;
                _jsvMap[jsValue] = id;
                return id;
            }
            else
            {
                var id = _freeIndex;
                ref var slot = ref _slots[id];
                _freeIndex = slot.next;
                slot.next = -1;
                slot.stringValue = stringValue;
                slot.jsValue = jsValue;
                _strMap[stringValue] = id;
                _jsvMap[jsValue] = id;
                return id;
            }
        }

        /// <summary>
        /// get corresponding jsValue of stringValue (only if cached before with AddValue/GetValue)
        /// </summary>
        public bool TryGetValue(string stringValue, out JSValue jsValue)
        {
            if (!_disposed && stringValue != null)
            {
                int slotIndex;
                if (_strMap.TryGetValue(stringValue, out slotIndex))
                {
                    ref readonly var slot = ref _slots[slotIndex];
                    jsValue = slot.jsValue;
                    return true;
                }
            }

            jsValue = JSApi.JS_UNDEFINED;
            return false;
        }

        /// <summary>
        /// get corresponding jsValue of stringValue (cache it if not exist)
        /// </summary>
        public bool GetValue(string stringValue, out JSValue jsValue)
        {
            if (!_disposed && stringValue != null)
            {
                int slotIndex;
                if (_strMap.TryGetValue(stringValue, out slotIndex))
                {
                    ref readonly var slot = ref _slots[slotIndex];
                    jsValue = slot.jsValue;
                    return true;
                }

                return AddValue(stringValue, out jsValue);
            }

            jsValue = JSApi.JS_UNDEFINED;
            return false;
        }

        public bool TryGetValue(JSValue jsValue, out string stringValue)
        {
            if (!_disposed && jsValue.IsString())
            {
                int slotIndex;
                if (_jsvMap.TryGetValue(jsValue, out slotIndex))
                {
                    ref readonly var slot = ref _slots[slotIndex];
                    stringValue = slot.stringValue;
                    return true;
                }
            }

            stringValue = null;
            return false;
        }

        public bool GetValue(JSValue jsValue, out string stringValue)
        {
            if (!_disposed && jsValue.IsString())
            {
                int slotIndex;
                if (_jsvMap.TryGetValue(jsValue, out slotIndex))
                {
                    ref readonly var slot = ref _slots[slotIndex];
                    stringValue = slot.stringValue;
                    return true;
                }

                return AddValue(jsValue, out stringValue);
            }

            stringValue = null;
            return false;
        }
    }
}
