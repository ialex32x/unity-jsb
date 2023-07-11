using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using MillisecondType = UInt64;

    public readonly struct TimerHandle
    {
        private readonly TimerManager _timerManager;
        private readonly SIndex _index;

        public SIndex index => _index;

        public bool isValid => _timerManager != null && _timerManager.IsValidTimer(this);

        public TimerHandle(TimerManager timerManager, in SIndex index)
        {
            _timerManager = timerManager;
            _index = index;
        }

        public void Invalidate() => _timerManager?.ClearTimer(_index);
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
        private struct InternalTimerData
        {
            public SIndex id;
            public IInvokable action;
            public MillisecondType rate;
            public MillisecondType expires;
            public bool loop;

            public override string ToString() => $"{nameof(InternalTimerData)}({id}: {rate} {loop})";
        }

        private class WheelSlot
        {
            private readonly List<SIndex> _timerIndices = new();

            public void Append(in SIndex timerIndex) => _timerIndices.Add(timerIndex);

            public void Move(List<SIndex> cache)
            {
                cache.AddRange(_timerIndices);
                _timerIndices.Clear();
            }

            public void Move(SList<SIndex> cache)
            {
                for (int i = 0, n = _timerIndices.Count; i < n; ++i)
                {
                    cache.Add(_timerIndices[i]);
                }
                _timerIndices.Clear();
            }

            public void Clear() => _timerIndices.Clear();
        }

        private class Wheel
        {
            private readonly uint _depth;
            private readonly MillisecondType _interval;
            private readonly MillisecondType _range;
            private readonly WheelSlot[] _slots;
            private uint _index;

            public MillisecondType range => _range;

            public Wheel(uint depth, MillisecondType interval, uint slots)
            {
                _depth = depth;
                _index = 0;
                _interval = interval;
                _range = _interval * slots;
                _slots = new WheelSlot[slots];
                for (var i = 0; i < slots; i++)
                {
                    _slots[i] = new WheelSlot();
                }
            }

            public void Add(MillisecondType delay, in SIndex timerIndex)
            {
                var offset = delay >= _interval ? (delay / _interval) - 1 : delay / _interval;
                // var offset = _depth == 0 ? Math.Max(0, (delay / _interval) - 1) : delay / _interval;
                // Diagnostics.Logger.Default.Warning("time wheel {0} insert delay:{1} index:{2} offset:{3}/{4}", this, delay, _index, offset, _slots.Length);
                var index = (uint)((_index + offset) % (MillisecondType)_slots.Length);
                _slots[index].Append(timerIndex);
                //UnityEngine.Debug.Assert(index > _index, "timer slot index");
                //UnityEngine.Debug.LogWarning($"[wheel#{_depth}:{_index}<range:{_range} _interval:{_interval}>] add timer#{timer.id} delay:{delay} to index: {index} offset: {offset}");
            }

            /// <summary>
            /// 返回 true 表示完成一轮循环
            /// </summary>
            public void Next(List<SIndex> activeIndices) => _slots[_index++].Move(activeIndices);
            
            public void Next(SList<SIndex> activeIndices) => _slots[_index++].Move(activeIndices);

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
            
            public override string ToString() => $"Wheel({_depth} index:{_index} range:{_range} interval:{_interval})";
        }

        private readonly int _mainThreadId;
        private readonly SList<InternalTimerData> _usedTimers = new();
        private readonly SList<SIndex> _activatedTimers = new();
        private readonly List<SIndex> _movingTimers = new();
        private readonly Wheel[] _wheels;
        private readonly uint _jiffies;
        private uint _timeSlice;
        private MillisecondType _elapsed;
        
        public MillisecondType now => _elapsed;

        public TimerManager(uint jiffies = 10, uint slots = 20, uint depth = 12)
        {
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
                _wheels[i] = new Wheel(i, jiffies * interval, slots);
            }
        }

        public InternalTimerInfo GetTimerInfo(in SIndex index)
        {
            if (_usedTimers.TryGetValue(index, out var data))
            {
                return new InternalTimerInfo(index, (int)data.rate, (int)data.expires, !data.loop);
            }
            return new InternalTimerInfo();
        }

        public void SetTimer(IInvokable fn, MillisecondType rate, bool isLoop = false, MillisecondType firstDelay = default)
        {
            SIndex index = default;
            SetTimer(ref index, fn, rate, isLoop, firstDelay);
        }

        public void SetTimer(ref TimerHandle handle, IInvokable fn, MillisecondType rate, bool isLoop = false, MillisecondType firstDelay = default)
        {
            var index = handle.index;
            SetTimer(ref index, fn, rate, isLoop, firstDelay);
            handle = new TimerHandle(this, index);
        }

        public void SetTimer(ref SIndex timerIndex, IInvokable fn, MillisecondType rate, bool isLoop = false, MillisecondType firstDelay = default)
        {
            CheckInternalState();

            _usedTimers.RemoveAt(timerIndex);
            var index = _usedTimers.Add(default);
            ref var timer = ref _usedTimers.UnsafeGetValueByRef(index);
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
            RearrangeTimer(timer.id, delay);
            // Diagnostics.Logger.Default.Debug($"[TimerManager] Add timer {timer}");
            timerIndex = index;
        }

        public bool IsValidTimer(in TimerHandle handle) => _usedTimers.IsValidIndex(handle.index);

        public bool ClearTimer(in SIndex timerIndex)
        {
            CheckInternalState();
            if (_usedTimers.RemoveAt(timerIndex))
            {
                return true;
            }

            return false;
        }

        public void Clear()
        {
            CheckInternalState();
            _timeSlice = default;
            _elapsed = default;
            _activatedTimers.Clear();
            _movingTimers.Clear();
            for (int wheelIndex = 0, wheelCount = _wheels.Length; wheelIndex < wheelCount; ++wheelIndex)
            {
                _wheels[wheelIndex].Clear();
            }
            _usedTimers.Clear();
        }

        public void Tick() => Update(_jiffies);

        public void Update(uint ms)
        {
            _timeSlice += ms;
            while (_timeSlice >= _jiffies)
            {
                _timeSlice -= _jiffies;
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
                                if (!_usedTimers.IsValidIndex(_movingTimers[i]))
                                {
                                    continue;
                                }
                                ref var timer = ref _usedTimers.UnsafeGetValueByRef(_movingTimers[i]);
                                if (timer.expires > _elapsed)
                                {
                                    RearrangeTimer(timer.id, timer.expires - _elapsed);
                                }
                                else
                                {
                                    _activatedTimers.Add(timer.id);
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
                sb.AppendFormat("{0}, ", w);
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

        private void RearrangeTimer(in SIndex timerId, MillisecondType delay)
        {
            var wheelCount = _wheels.Length;
            for (var i = 0; i < wheelCount; i++)
            {
                var wheel = _wheels[i];
                if (delay < wheel.range)
                {
                    wheel.Add(delay, timerId);
                    return;
                }
            }

            Diagnostics.Logger.Default.Error("out of time range {0}", delay);
            _wheels[wheelCount - 1].Add(delay, timerId);
        }

        private void InvokeTimers()
        {
            do
            {
                var index = _activatedTimers.firstIndex;
                if (!_activatedTimers.TryRemoveAt(index, out var pendingTimerIndex))
                {
                    return;
                }
                if (!_usedTimers.IsValidIndex(pendingTimerIndex))
                {
                    continue;
                }

                ref var timer = ref _usedTimers.UnsafeGetValueByRef(pendingTimerIndex);
                // Diagnostics.Logger.Default.Debug("timer active {0}", timer);
                timer.action.Invoke();
                if (pendingTimerIndex == timer.id)
                {
                    if (timer.loop)
                    {
                        // 确认该 timer 仍然有效 (没有在回调中 Remove), 刷新下一次触发时间
                        timer.expires = timer.rate + _elapsed;
                        RearrangeTimer(timer.id, timer.rate);
                    }
                    else
                    {
                        ClearTimer(pendingTimerIndex);
                    }
                }
            } while (true);
        }
    }
}
