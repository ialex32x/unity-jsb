using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using MillisecondType = System.UInt64;

    using TimerCallback = Invokable;

    public readonly struct TimerHandle
    {
        private readonly TimerManager _timerManager;
        private readonly SIndex _index;

        public readonly SIndex index => _index;

        public readonly bool isValid => _timerManager != null && _timerManager.IsValidTimer(this);

        public TimerHandle(TimerManager timerManager, in SIndex index)
        {
            _timerManager = timerManager;
            _index = index;
        }

        public readonly void Invalidate()
        {
            _timerManager.ClearTimer(_index);
        }
    }

    public readonly struct InternalTimerInfo
    {
        public readonly SIndex index;
        public readonly int delay;
        public readonly int deadline;
        public readonly bool once;

        public InternalTimerInfo(in SIndex index, int delay, int deadline, bool once)
        {
            this.index = index;
            this.delay = delay;
            this.deadline = deadline;
            this.once = once;
        }
    }

    public class TimerManager
    {
        public class InternalTimerData
        {
            public SIndex id;
            public TimerCallback action;
            public MillisecondType rate;
            public MillisecondType expires;
            public bool loop;

            public override string ToString()
            {
                var loop = this.loop ? "loop" : "";
                return $"{nameof(InternalTimerData)}({id}: {rate} {loop})";
            }
        }

        private class WheelSlot
        {
            private List<SIndex> _timerIndices = new List<SIndex>();

            public void Append(in SIndex timerIndex)
            {
                _timerIndices.Add(timerIndex);
            }

            public void Move(List<SIndex> cache)
            {
                cache.AddRange(_timerIndices);
                _timerIndices.Clear();
            }

            public void Clear()
            {
                _timerIndices.Clear();
            }
        }

        private class Wheel
        {
            private uint _depth;
            private uint _jiffies;
            private MillisecondType _interval;
            private MillisecondType _range;
            private uint _index;
            private WheelSlot[] _slots;

            public uint depth { get { return _depth; } }

            public uint index { get { return _index; } }

            public MillisecondType range { get { return _range; } }

            public Wheel(uint depth, uint jiffies, MillisecondType interval, uint slots)
            {
                _depth = depth;
                _index = 0;
                _jiffies = jiffies;
                _interval = interval;
                _range = _interval * slots;
                _slots = new WheelSlot[slots];
                for (var i = 0; i < slots; i++)
                {
                    _slots[i] = new WheelSlot();
                }
            }

            public uint Add(MillisecondType delay, in SIndex timerIndex)
            {
                var offset = delay >= _interval ? (delay / _interval) - 1 : delay / _interval;
                // var offset = _depth == 0 ? Math.Max(0, (delay / _interval) - 1) : delay / _interval;
                // Diagnostics.Logger.Default.Warning("time wheel {0} insert delay:{1} index:{2} offset:{3}/{4}", this, delay, _index, offset, _slots.Length);
                var index = (uint)((_index + offset) % (MillisecondType)_slots.Length);
                _slots[index].Append(timerIndex);
                //UnityEngine.Debug.Assert(index > _index, "timer slot index");
                //UnityEngine.Debug.LogWarning($"[wheel#{_depth}:{_index}<range:{_timerange} _interval:{_interval}>] add timer#{timer.id} delay:{delay} to index: {index} offset: {offset}");
                return index;
            }

            /// <summary>
            /// 返回 true 表示完成一轮循环
            /// </summary>
            public void Next(List<SIndex> activeIndices)
            {
                _slots[_index++].Move(activeIndices);
            }

            public bool Round()
            {
                if (_index == _slots.Length)
                {
                    _index = 0;
                    return true;
                }
                return false;
            }

            public void Clear()
            {
                _index = 0;
                for (int i = 0, count = _slots.Length; i < count; ++i)
                {
                    _slots[i].Clear();
                }
            }

            public override string ToString()
            {
                return $"Wheel({_depth} index:{_index} range:{_range} interval:{_interval})";
            }
        }

        private int _mainThreadId;
        private int _freeTimerCap;
        private List<InternalTimerData> _freeTimers = new List<InternalTimerData>();
        private SList<InternalTimerData> _usedTimers = new SList<InternalTimerData>();
        private Wheel[] _wheels;
        private uint _timeslice;
        private MillisecondType _elapsed;
        private uint _jiffies;
        private List<SIndex> _activatedTimers = new List<SIndex>();
        private List<SIndex> _movingTimers = new List<SIndex>();

        public uint jiffies => _jiffies;

        public TimerManager(uint jiffies = 10, uint slots = 20, uint depth = 12)
        {
            _freeTimerCap = 100;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _jiffies = jiffies;
            _wheels = new Wheel[depth];
            for (uint i = 0; i < depth; i++)
            {
                uint interval = 1;
                for (var j = 0; j < i; j++)
                {
                    interval *= slots;
                }
                _wheels[i] = new Wheel(i, jiffies, jiffies * interval, slots);
            }
        }

        public MillisecondType now => _elapsed;

        public InternalTimerInfo GetTimerInfo(in SIndex index)
        {
            if (_usedTimers.TryGetValue(index, out var data))
            {
                return new InternalTimerInfo(index, (int)data.rate, (int)data.expires, !data.loop);
            }
            return new InternalTimerInfo();
        }

        public void SetTimer(ref TimerHandle handle, TimerCallback fn, MillisecondType rate, bool isLoop = false, MillisecondType firstDelay = default)
        {
            var index = handle.index;
            SetTimer(ref index, fn, rate, isLoop, firstDelay);
            handle = new TimerHandle(this, index);
        }

        public void SetTimer(ref SIndex timerIndex, TimerCallback fn, MillisecondType rate, bool isLoop = false, MillisecondType firstDelay = default)
        {
            CheckInternalState();

            if (_usedTimers.TryRemoveAt(timerIndex, out var activeTimer))
            {
                RecycleFreeTimer(activeTimer);
            }

            var timer = NewTimerInternal();
            var index = _usedTimers.Add(timer);
            var delay = firstDelay > 0 ? firstDelay : rate;
            timer.rate = rate;
            timer.expires = delay + _elapsed;
            timer.action = fn;
            timer.id = index;
            timer.loop = isLoop;

            if (delay == 0)
            {
                Diagnostics.Logger.Default.Warning("timer with no delay will initially be processed after a single tick");
            }
            RearrangeTimer(timer, delay);
            // Diagnostics.Logger.Default.Debug($"[TimerManager] Add timer {timer}");
            timerIndex = index;
        }

        public bool IsValidTimer(in TimerHandle handle) => _usedTimers.IsValidIndex(handle.index);

        private void RecycleFreeTimer(InternalTimerData timer)
        {
            timer.id = default;
            timer.action = default;
            if (_freeTimers.Count < _freeTimerCap)
            {
                _freeTimers.Add(timer);
            }
        }

        public bool ClearTimer(in SIndex timerIndex)
        {
            CheckInternalState();
            if (_usedTimers.TryRemoveAt(timerIndex, out var timer))
            {
                RecycleFreeTimer(timer);
                return true;
            }

            Diagnostics.Logger.Default.Warning("invalid timer index {0}", timerIndex);
            return false;
        }

        public void Clear()
        {
            CheckInternalState();
            _timeslice = default;
            _elapsed = default;
            _activatedTimers.Clear();
            _movingTimers.Clear();
            for (int wheelIndex = 0, wheelCount = _wheels.Length; wheelIndex < wheelCount; ++wheelIndex)
            {
                _wheels[wheelIndex].Clear();
            }
            var it = _usedTimers.GetUnsafeEnumerator();
            while (it.MoveNext())
            {
                RecycleFreeTimer(it.Current);
                it.Remove();
            }
        }

        public void Tick() => Update(_jiffies);

        public void Update(uint ms)
        {
            _timeslice += ms;
            while (_timeslice >= _jiffies)
            {
                _timeslice -= _jiffies;
                _elapsed += _jiffies;
                _wheels[0].Next(_activatedTimers);
                for (int wheelIndex = 0, wheelCount = _wheels.Length; wheelIndex < wheelCount; ++wheelIndex)
                {
                    if (_wheels[wheelIndex].Round())
                    {
                        if (wheelIndex != wheelCount - 1)
                        {
                            _wheels[wheelIndex + 1].Next(_movingTimers);
                            for (int i = 0, count2 = _movingTimers.Count; i < count2; ++i)
                            {
                                if (_usedTimers.TryGetValue(_movingTimers[i], out var timer))
                                {
                                    if (timer.expires > _elapsed)
                                    {
                                        RearrangeTimer(timer, timer.expires - _elapsed);
                                    }
                                    else
                                    {
                                        _activatedTimers.Add(timer.id);
                                    }
                                }
                            }
                            _movingTimers.Clear();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            InvokeTimers();
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var w in _wheels)
            {
                sb.AppendFormat("{0}, ", w.ToString());
            }
            return sb.ToString();
        }

        [Conditional("JSB_DEBUG")]
        private void CheckInternalState()
        {
            if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new Exception("TimerManager is only available in main thread");
            }
        }

        private void RearrangeTimer(InternalTimerData timer, MillisecondType delay)
        {
            var wheelCount = _wheels.Length;
            for (var i = 0; i < wheelCount; i++)
            {
                var wheel = _wheels[i];
                if (delay < wheel.range)
                {
                    wheel.Add(delay, timer.id);
                    return;
                }
            }

            Diagnostics.Logger.Default.Error("out of time range {0}", delay);
            _wheels[wheelCount - 1].Add(delay, timer.id);
        }

        private InternalTimerData NewTimerInternal()
        {
            var poolIndex = _freeTimers.Count - 1;
            InternalTimerData timer;
            if (poolIndex >= 0)
            {
                timer = _freeTimers[poolIndex];
                _freeTimers.RemoveAt(poolIndex);
            }
            else
            {
                timer = new InternalTimerData();
            }
            return timer;
        }

        private void InvokeTimers()
        {
            for (int i = 0, cc = _activatedTimers.Count; i < cc; ++i)
            {
                var pendingTimerIndex = _activatedTimers[i];
                if (_usedTimers.TryGetValue(pendingTimerIndex, out var timer))
                {
                    // Diagnostics.Logger.Default.Debug("timer active {0}", timer);
                    timer.action.Invoke();
                    if (pendingTimerIndex == timer.id)
                    {
                        if (timer.loop)
                        {
                            // 确认该 timer 仍然有效 (没有在回调中 Remove), 刷新下一次触发时间
                            timer.expires = timer.rate + _elapsed;
                            RearrangeTimer(timer, timer.rate);
                        }
                        else
                        {
                            ClearTimer(pendingTimerIndex);
                        }
                    }
                }
                else
                {
                    Diagnostics.Logger.Default.Warning("timer active (invalid) {0}", timer);
                }
            }
            _activatedTimers.Clear();
        }
    }
}
