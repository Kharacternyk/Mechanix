﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Mechanix
{
    /// <summary>
    /// Represents a set of <see cref="PhysicalContext{TEntityKey}"/> 
    /// determined values in certain time range
    /// </summary>
    /// <typeparam name="TEntityKey"> 
    /// Type of <see cref="PhysicalContext{TEntityKey}"/> entities keys
    /// </typeparam>
    /// <typeparam name="TValue"> Type of observed value </typeparam>
    public sealed class ContextTracker<TEntityKey, TValue> : ContextObserver<TEntityKey>, IReadOnlyDictionary<ulong, TValue>
    {
        readonly Func<PhysicalContext<TEntityKey>, TValue> _func;
        readonly List<TValue> _values;
        ulong _currTimer;
        /// <summary>
        /// Interval of the data recording (from observable context)
        /// </summary>
        public ulong Interval { get; }

        public ulong LastRecordTick => ObservationBeginTick + (ulong)(_values.Count - 1) * Interval;
        public double LastRecordTime => LastRecordTick * ObservableContext.TimePerTick;
        public TValue LastRecord => _values[_values.Count - 1];

        public event EventHandler OnRecord;

        public ContextTracker
        (
            PhysicalContext<TEntityKey> observableContext,
            Func<PhysicalContext<TEntityKey>, TValue> valueFunc,
            ulong interval = 1
        )
        : base(observableContext)
        {
            if (interval == 0) throw new ArgumentOutOfRangeException(nameof(interval), "ContextTracker interval should be non-zero");
            _func = valueFunc;
            Interval = interval;
            _values = new List<TValue>();

            _currTimer = 0;
            _values.Add(_func(ObservableContext));
        }

        protected override void OnDisposed()
        {
            OnRecord = null;
        }

        protected override void Observe()
        {
            if (_currTimer == Interval - 1)
            {
                _values.Add(_func(ObservableContext));
                _currTimer = 0;
                OnRecord?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ++_currTimer;
            }
        }

        public IEnumerable<ulong> Keys
        {
            get
            {
                for (uint i = 0; i < _values.Count; ++i)
                {
                    yield return ObservationBeginTick + i * Interval;
                }
                yield break;
            }
        }

        public IEnumerable<TValue> Values => _values;

        public int Count => _values.Count;

        /// <summary>
        /// Observed value on certain tick of <see cref="ObservableContext"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public TValue this[ulong ticks]
        {
            get
            {
                if (ticks < ObservationBeginTick)
                {
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(ticks),
                        $"ContextTracker doesn't contain any information about the state of observable context at {ticks} " +
                        $"because observation begin at {ObservationBeginTick} tick."
                    );
                }
                if (ticks > LastObservedTick)
                {
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(ticks),
                        IsActive ?
                        $"ContextTracker doesn't gather any information about " +
                        $"the state of observable context at {ticks} tick because last observation was performed at {LastObservedTick}."
                        :
                        $"ContextTracker is disposed, so it hasn't information " +
                        $"about state of observable context at {ticks} because LastObservedTick is {LastObservedTick}."
                    );
                }
                if ((ticks - ObservationBeginTick) % Interval != 0)
                {
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(ticks),
                        $"ContextTracker collects data only each {Interval} tick. Can't access data at {ticks}" +
                        $"tick, the closest accessible tick value is {ticks - (ticks - ObservationBeginTick) % Interval}." +
                        $"If you want to get the closest value by default use GetApproximately method instead."
                    );
                }
                
                return _values[(int)((ticks - ObservationBeginTick) / Interval)];
            }
        }

        /// <summary>
        /// Observed value on certain point of time <see cref="ObservableContext"/>
        /// <para>
        /// For better precision use navigation by ticks via <see cref="this[ulong]"/>
        /// </para>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TValue GetApproximately(double time)
        {
            ulong ticks = (ulong)Math.Round(time / ObservableContext.TimePerTick);
            return _values[(int)((ticks - ObservationBeginTick) / Interval)];
        }

        public bool ContainsKey(ulong key)
        {
            return ObservationBeginTick <= key && key <= LastObservedTick && (key - ObservationBeginTick) % Interval == 0;
        }

        public bool TryGetValue(ulong key, out TValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<ulong, TValue>> GetEnumerator()
        {
            for (uint i = 0; i < _values.Count; ++i)
            {
                var key = ObservationBeginTick + i * Interval;
                var value = _values[(int)i];
                yield return new KeyValuePair<ulong, TValue>(key, value);
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }
    }
}
