using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QuickJS.Utils
{
    using QuickJS.Diagnostics;

    /// <summary>
    /// Stable Index
    /// </summary>
    public readonly struct SIndex : IEquatable<SIndex>
    {
        public static readonly SIndex None = default;

        public readonly int index;
        public readonly int revision;

        public SIndex(int index, int revision)
        {
            this.index = index;
            this.revision = revision;
        }

        public bool Equals(in SIndex other) => this == other;

        public bool Equals(SIndex other) => this == other;

        public override bool Equals(object obj) => obj is SIndex other && this == other;

        public override int GetHashCode() => index ^ revision;

        public override string ToString() => index.ToString();

        public static bool operator ==(SIndex a, SIndex b) => a.index == b.index && a.revision == b.revision;

        public static bool operator !=(SIndex a, SIndex b) => a.index != b.index || a.revision != b.revision;
    }

    public interface SListAccess
    {
        void Lock();
        void Unlock();
        bool IsValidIndex(in SIndex index);
        bool UnsafeSetValue(in SIndex index);
        bool RemoveAt(in SIndex index);
        void Clear();
    }

    /// <summary>
    /// Stable Indexed Array List.
    /// * 可以使用稳定索引快速访问元素, 元素在列表中的稳定索引不会发生变化.
    /// * 可以使用稳定索引快速删除元素.
    /// * 元素移除后索引位置会由下一个被添加的元素复用
    /// * 可以使用 foreach 顺序迭代此列表, 没有额外的查找开销. (即 GetEnumerator)
    /// * 可以使用稳定索引安全地访问, 如果该稳定索引已失效, 则抛出异常.
    /// * 可以使用 Lock 阻止所有修改操作 (Unload 解锁)
    /// * 此列表的使用稳定索引(SIndex)的效率较高, 随机直接索引(int)访问的效率较差.
    /// </summary>
    public class SList<T> : SListAccess, IEnumerable<T>
    {
        private struct Slot
        {
            // 空闲状态下指向下一个空闲栏位, 否则指向下一个有效栏位
            public int next;
            // 空闲状态下无效, 在有效栏位时代表前一个有效栏位
            public int previous;
            // 记录覆盖次数
            public int revision;
            // 单独记录有效性, 增加 revision 溢出时的可靠性
            public bool isValid;
            public T value;

            public override string ToString()
            {
                return $"valid: {isValid} rev: {revision} prev: {previous} next: {next} value: {value}";
            }
        }

        private int _lock;
        private int _freeIndex;
        private int _usedSize;
        private Slot[] _slots;

        private int _firstIndex;
        private int _lastIndex;
        private int _version;

        public int Count => _usedSize;

        public int Capacity => _slots.Length;

        public bool isLocked => _lock > 0;

        public SIndex firstIndex => _firstIndex >= 0 ? new SIndex(_firstIndex, _slots[_firstIndex].revision) : SIndex.None;
        
        public SIndex lastIndex => _lastIndex >= 0 ? new SIndex(_lastIndex, _slots[_lastIndex].revision) : SIndex.None;
        
        public T this[in SIndex index]
        {
            get
            {
                if (index.index >= 0 && index.index < this._slots.Length)
                {
                    ref var slot = ref _slots[index.index];

                    if (slot.revision == index.revision)
                    {
                        return slot.value;
                    }
                    throw new IndexOutOfRangeException("accessing with expired index");
                }

                throw new IndexOutOfRangeException();
            }

            set
            {
                if (!UnsafeSetValue(index, value))
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= 0 && index < this._usedSize)
                {
                    var current = _firstIndex;
                    while (current >= 0)
                    {
                        ref var slot = ref this._slots[current];
                        Assert.Debug(slot.isValid);
                        if (index-- == 0)
                        {
                            return slot.value;
                        }
                        current = slot.next;
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set
            {
                if (index >= 0 && index < this._usedSize)
                {
                    var current = _firstIndex;
                    while (current >= 0)
                    {
                        ref var slot = ref this._slots[current];
                        Assert.Debug(slot.isValid);
                        if (index-- == 0)
                        {
                            slot.value = value;
                            return;
                        }
                        current = slot.next;
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>)this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();

        public SList<T>.Enumerator GetEnumerator() => new SList<T>.Enumerator(this);

        public SList<T>.UnsafeEnumerator GetUnsafeEnumerator() => new SList<T>.UnsafeEnumerator(this);

        public SList<T>.StableIndexEnumerator GetStableIndexEnumerator() => new SList<T>.StableIndexEnumerator(this);

        public SList()
        : this(8)
        {
        }

        public SList(int initialCapacity)
        {
            _lock = 0;
            _freeIndex = -1;
            _firstIndex = -1;
            _lastIndex = -1;
            _slots = new Slot[0];
            GrowIfNeeded(initialCapacity);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var current = this._firstIndex;

            sb.Append('[');
            while (current >= 0)
            {
                ref var slot = ref this._slots[current];
                sb.Append(slot.value);
                if (current != _lastIndex)
                {
                    sb.Append(", ");
                }
                current = slot.next;
            }
            sb.Append(']');
            return sb.ToString();
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("[");
            sb.AppendLine($"\tfirst: {_firstIndex}");
            sb.AppendLine($"\tlast: {_lastIndex}");
            sb.AppendLine($"\tfree: {_freeIndex}");
            sb.AppendLine($"\tused: {_usedSize}");
            sb.AppendLine($"\tlock: {_lock}");
            for (int i = 0, count = _slots.Length; i < count; i++)
            {
                ref var slot = ref this._slots[i];
                if (i != count)
                {
                    sb.AppendLine($"\t{i} = {slot}, ");
                }
                else
                {
                    sb.AppendLine($"\t{i} = {slot}");
                }
            }
            sb.AppendLine("]");
            return sb.ToString();
        }

        public T[] ToArray()
        {
            var array = new T[_usedSize];
            var fillIndex = 0;
            var current = this._firstIndex;

            while (current >= 0)
            {
                ref var slot = ref this._slots[current];
                Assert.Debug(slot.isValid);
                array[fillIndex++] = slot.value;
                current = slot.next;
            }

            Assert.Debug(fillIndex == _usedSize);
            return array;
        }

        private void GrowIfNeeded(int newCount)
        {
            var newSize = this._usedSize + newCount;
            var oldSize = this._slots.Length;
            if (newSize > oldSize)
            {
                newSize = Math.Max(Math.Max(oldSize * 2, 4), newSize);

                Array.Resize(ref this._slots, newSize);
                for (var i = oldSize; i < newSize; ++i)
                {
                    ref var slot = ref this._slots[i];
                    slot.next = _freeIndex;
                    this._freeIndex = i;
                }
            }
        }

        public void Lock()
        {
            ++_lock;
        }

        public void Unlock()
        {
            --_lock;
            Assert.Debug(_lock >= 0);
        }

        public bool IsValidIndex(in SIndex index)
        {
            if (index.index >= 0 && index.index < this._slots.Length)
            {
                ref var slot = ref _slots[index.index];

                if (slot.isValid && slot.revision == index.revision)
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(in SIndex index, out T value)
        {
            if (index.index >= 0 && index.index < this._slots.Length)
            {
                ref var slot = ref _slots[index.index];

                if (slot.isValid && slot.revision == index.revision)
                {
                    value = slot.value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public ref T UnsafeGetValueByRef(SIndex index)
        {
            if (index.index >= 0 && index.index < this._slots.Length)
            {
                ref var slot = ref _slots[index.index];

                if (slot.revision == index.revision)
                {
                    return ref slot.value;
                }

                throw new IndexOutOfRangeException("accessing with expired index");
            }

            throw new KeyNotFoundException("accessing with invalid index");
        }

        public T UnsafeGetValue(in SIndex index)
        {
            if (index.index >= 0 && index.index < this._slots.Length)
            {
                ref var slot = ref _slots[index.index];

                if (slot.isValid && slot.revision == index.revision)
                {
                    return slot.value;
                }
            }
            return default;
        }

        public bool UnsafeSetValue(in SIndex index)
        {
            return UnsafeSetValue(index, default);
        }

        public bool UnsafeSetValue(in SIndex index, T value)
        {
            if (index.index >= 0 && index.index < this._slots.Length)
            {
                ref var slot = ref _slots[index.index];

                if (slot.isValid && slot.revision == index.revision)
                {
                    slot.value = value;
                    return true;
                }
            }

            return false;
        }

        public SIndex UnsafeIndexAt(int index, out T value)
        {
            Assert.Debug(index >= 0 && index < _usedSize);

            var current = _firstIndex;
            while (current >= 0)
            {
                ref var slot = ref this._slots[current];
                Assert.Debug(slot.isValid);
                if (index-- == 0)
                {
                    value = slot.value;
                    return new(current, slot.revision);
                }
                current = slot.next;
            }

            value = default;
            return SIndex.None;
        }

        public SIndex UnsafeAdd(T value)
        {
            GrowIfNeeded(1);
            Assert.Debug(_freeIndex != -1);

            var index = _freeIndex;
            ref var slot = ref _slots[index];

            // safer to skip SIndex.None
            slot.revision = slot.revision == -1 ? 1 : slot.revision + 1;
            slot.value = value;
            slot.isValid = true;
            _freeIndex = slot.next;
            slot.next = -1;
            slot.previous = _lastIndex;
            ++_usedSize;
            if (_lastIndex >= 0)
            {
                ref var lastSlot = ref _slots[_lastIndex];
                lastSlot.next = index;
            }
            if (_firstIndex < 0)
            {
                _firstIndex = index;
            }
            _lastIndex = index;
            ++_version;
            return new SIndex(index, slot.revision);
        }

        public SIndex Add(T value)
        {
            Assert.Debug(_lock == 0);
            return UnsafeAdd(value);
        }

        public void Insert(in SIndex index, T value)
        {
            Assert.Debug(_lock == 0);
            if (index.index < 0 || index.index >= _slots.Length)
            {
                throw new IndexOutOfRangeException();
            }

            ref var slot = ref this._slots[index.index];
            if (!slot.isValid || slot.revision != index.revision)
            {
                throw new IndexOutOfRangeException("accessing with expired index");
            }

            var newIndex = _freeIndex;
            ref var newSlot = ref _slots[newIndex];

            newSlot.revision = newSlot.revision == -1 ? 1 : newSlot.revision + 1;
            newSlot.value = value;
            newSlot.isValid = true;
            _freeIndex = newSlot.next;
            newSlot.next = index.index;
            newSlot.previous = slot.previous;
            slot.previous = newIndex;
            if (newSlot.previous >= 0)
            {
                ref var previousSlot = ref _slots[newSlot.previous];
                previousSlot.next = newIndex;
            }
            ++_usedSize;
            if (_firstIndex == index.index)
            {
                _firstIndex = newIndex;
            }
            ++_version;
        }

        public void Insert(int index, T value)
        {
            Assert.Debug(index >= 0 && index <= _usedSize);
            if (_usedSize == index)
            {
                Add(value);
                return;
            }

            Assert.Debug(_lock == 0);
            GrowIfNeeded(1);
            Assert.Debug(_freeIndex != -1);

            var current = _firstIndex;
            while (current >= 0)
            {
                ref var slot = ref this._slots[current];
                Assert.Debug(slot.isValid);
                if (index-- == 0)
                {
                    var newIndex = _freeIndex;
                    ref var newSlot = ref _slots[newIndex];

                    newSlot.revision = newSlot.revision == -1 ? 1 : newSlot.revision + 1;
                    newSlot.value = value;
                    newSlot.isValid = true;
                    _freeIndex = newSlot.next;
                    newSlot.next = current;
                    newSlot.previous = slot.previous;
                    slot.previous = newIndex;
                    if (newSlot.previous >= 0)
                    {
                        ref var previousSlot = ref _slots[newSlot.previous];
                        previousSlot.next = newIndex;
                    }
                    ++_usedSize;
                    if (_firstIndex == current)
                    {
                        _firstIndex = newIndex;
                    }
                    ++_version;
                    break;
                }
                current = slot.next;
            }
            ++_version;
        }

        public bool Remove(T item)
        {
            return RemoveAt(StableIndexOf(item));
        }

        public bool RemoveAt(int index)
        {
            if (index >= 0 && index < _usedSize)
            {
                var current = _firstIndex;
                while (current >= 0)
                {
                    ref var slot = ref this._slots[current];
                    Assert.Debug(slot.isValid);
                    if (index-- == 0)
                    {
                        return RemoveAt(new SIndex(current, slot.revision));
                    }
                    current = slot.next;
                }
                Assert.Never();
            }

            return false;
        }

        // 按绝对位置删除
        public bool UnsafeRemoveSlotAt(int index)
        {
            if (index >= 0 && index < _slots.Length)
            {
                ref var slot = ref _slots[index];
                return RemoveAt(new SIndex(index, slot.revision));
            }
            return false;
        }

        public bool RemoveAt(in SIndex index)
        {
            return TryRemoveAt(index, out var value);
        }

        public bool TryRemoveAt(int index, out T value)
        {
            if (index >= 0 && index < this._usedSize)
            {
                var current = _firstIndex;
                while (current >= 0)
                {
                    ref var slot = ref this._slots[current];
                    Assert.Debug(slot.isValid);
                    if (index-- == 0)
                    {
                        return TryRemoveAt(new SIndex(current, slot.revision), out value);
                    }
                    current = slot.next;
                }
            }

            value = default;
            return false;
        }

        public bool TryRemoveAt(in SIndex index, out T value)
        {
            if (index.index >= 0 && index.index < this._slots.Length)
            {
                ref var slot = ref _slots[index.index];

                if (slot.isValid && slot.revision == index.revision)
                {
                    Assert.Debug(_lock == 0);

                    var next = slot.next;
                    var previous = slot.previous;

                    value = slot.value;
                    slot.value = default;
                    slot.next = _freeIndex;
                    slot.isValid = false;
                    _freeIndex = index.index;
                    --_usedSize;
                    ++_version;

                    if (next >= 0)
                    {
                        ref var nextSlot = ref _slots[next];
                        nextSlot.previous = previous;
                    }
                    if (previous >= 0)
                    {
                        ref var previousSlot = ref _slots[previous];
                        previousSlot.next = next;
                    }
                    if (_firstIndex == index.index)
                    {
                        _firstIndex = next;
                    }
                    if (_lastIndex == index.index)
                    {
                        _lastIndex = previous;
                    }
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Clear()
        {
            if (_usedSize > 0)
            {
                Assert.Debug(_lock == 0);
                Assert.Debug(_firstIndex >= 0);

                while (_firstIndex >= 0)
                {
                    var index = _firstIndex;
                    ref var slot = ref this._slots[index];

                    Assert.Debug(slot.isValid);
                    _firstIndex = slot.next;
                    slot.next = _freeIndex;
                    slot.value = default;
                    slot.isValid = false;
                    // ++slot.revision;
                    _freeIndex = index;
                }
                Assert.Debug(_firstIndex == -1);
                _lastIndex = -1;
                _usedSize = 0;
                ++_version;
            }
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public int IndexOf(T item)
        {
            if ((object)item == null)
            {
                var current = _firstIndex;
                while (current >= 0)
                {
                    ref var slot = ref this._slots[current];
                    Assert.Debug(slot.isValid);
                    if ((object)slot.value == null)
                    {
                        return current;
                    }
                    current = slot.next;
                }
            }
            else
            {
                EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
                var current = _firstIndex;
                while (current >= 0)
                {
                    ref var slot = ref this._slots[current];
                    Assert.Debug(slot.isValid);
                    if (equalityComparer.Equals(slot.value, item))
                    {
                        return current;
                    }
                    current = slot.next;
                }
            }

            return -1;
        }

        public int LastIndexOf(T item)
        {
            if ((object)item == null)
            {
                var current = _lastIndex;
                while (current >= 0)
                {
                    ref var slot = ref this._slots[current];
                    Assert.Debug(slot.isValid);
                    if ((object)slot.value == null)
                    {
                        return current;
                    }
                    current = slot.previous;
                }
            }
            else
            {
                EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
                var current = _lastIndex;
                while (current >= 0)
                {
                    ref var slot = ref this._slots[current];
                    Assert.Debug(slot.isValid);
                    if (equalityComparer.Equals(slot.value, item))
                    {
                        return current;
                    }
                    current = slot.previous;
                }
            }

            return -1;
        }

        public SIndex StableIndexOf(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                ref var slot = ref this._slots[index];
                return new SIndex(index, slot.revision);
            }

            return SIndex.None;
        }

        public SIndex LastStableIndexOf(T item)
        {
            var index = LastIndexOf(item);
            if (index >= 0)
            {
                ref var slot = ref this._slots[index];
                return new SIndex(index, slot.revision);
            }

            return SIndex.None;
        }

        public SIndex Find(Func<T, bool> pred)
        {
            var current = this._firstIndex;

            while (current >= 0)
            {
                ref var slot = ref this._slots[current];
                Assert.Debug(slot.isValid);
                if (pred(slot.value))
                {
                    return new SIndex(current, slot.revision);
                }
                current = slot.next;
            }

            return default;
        }

        public R Sum<R>(Func<T, R, R> fn)
        {
            var current = this._firstIndex;
            var result = default(R);

            while (current >= 0)
            {
                ref var slot = ref this._slots[current];
                Assert.Debug(slot.isValid);
                result = fn(slot.value, result);
                current = slot.next;
            }

            return result;
        }

        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private SList<T> _list;
            private int _current;
            private int _next;
            private int _version;

            public Enumerator(SList<T> pool)
            {
                this._list = pool;
                this._current = -1;
                this._next = pool._firstIndex;
                this._version = pool._version;
            }

            private void VerifyState()
            {
                if (this._list == null)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this._version != this._list._version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
            }

            public T Current
            {
                get
                {
                    VerifyState();
                    return this._list._slots[_current].value;
                }
            }

            public bool MoveNext()
            {
                VerifyState();

                this._current = this._next;
                if (this._current < 0)
                {
                    return false;
                }
                this._next = this._list._slots[this._current].next;
                return true;
            }

            public void Reset()
            {
                this._current = -1;
                this._next = this._list._firstIndex;
            }

            public void Dispose() => this._list = null;

            void IDisposable.Dispose() => this._list = null;

            object IEnumerator.Current
            {
                get
                {
                    VerifyState();
                    ref var slot = ref this._list._slots[_current];
                    Assert.Debug(slot.isValid);
                    return (object)slot.value;
                }
            }
        }

        public struct UnsafeEnumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private SList<T> _list;
            private int _current;
            private int _next;

            public UnsafeEnumerator(SList<T> pool)
            {
                this._list = pool;
                this._current = -1;
                this._next = pool._firstIndex;
            }

            public SIndex Index
            {
                get
                {
                    ref var slot = ref this._list._slots[_current];
                    return new(_current, slot.revision);
                }
            }

            public T Current
            {
                get
                {
                    ref var slot = ref this._list._slots[_current];
                    Assert.Debug(slot.isValid);
                    return slot.value;
                }
            }

            public bool MoveNext()
            {
                this._current = this._next;
                if (this._current < 0)
                {
                    return false;
                }
                this._next = this._list._slots[this._current].next;
                return true;
            }

            public void Remove()
            {
                this._list.UnsafeRemoveSlotAt(_current);
                this._current = -1;
            }

            public void Reset()
            {
                this._current = -1;
                this._next = this._list._firstIndex;
            }

            public void Dispose() => this._list = null;

            void IDisposable.Dispose() => this._list = null;

            object IEnumerator.Current => (object)this._list._slots[_current].value;
        }

        public struct StableIndexEnumerator : IEnumerator<SIndex>, IDisposable, IEnumerator
        {
            private SList<T> _list;
            private int _current;
            private int _next;
            private int _version;

            public StableIndexEnumerator(SList<T> pool)
            {
                this._list = pool;
                this._current = -1;
                this._next = pool._firstIndex;
                this._version = pool._version;
            }

            private void VerifyState()
            {
                if (this._list == null)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this._version != this._list._version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
            }

            public SIndex Current
            {
                get
                {
                    VerifyState();
                    ref var slot = ref this._list._slots[_current];
                    Assert.Debug(slot.isValid);
                    return new SIndex(_current, slot.revision);
                }
            }

            public T Value => this._list[this.Current];

            public bool MoveNext()
            {
                VerifyState();

                this._current = this._next;
                if (this._current < 0)
                {
                    return false;
                }
                this._next = this._list._slots[this._current].next;
                return true;
            }

            public void Reset()
            {
                this._current = -1;
                this._next = this._list._firstIndex;
            }

            public void Dispose() => this._list = null;

            void IDisposable.Dispose() => this._list = null;

            object IEnumerator.Current
            {
                get
                {
                    VerifyState();
                    ref var slot = ref this._list._slots[_current];
                    Assert.Debug(slot.isValid);
                    return (object)new SIndex(_current, slot.revision);
                }
            }
        }
    }
}