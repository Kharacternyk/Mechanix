using System;
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
        Func<PhysicalContext<TEntityKey>, TValue> _func;
        List<TValue> _values;

        public ContextTracker
        (
            PhysicalContext<TEntityKey> observableContext,
            Func<PhysicalContext<TEntityKey>, TValue> valueFunc
        )
        : base(observableContext)
        {
            _func = valueFunc;

            _values = new List<TValue>();
            Observe();
        }

        protected override void Observe()
        {
            _values.Add(_func(ObservableContext));
        }

        public IEnumerable<ulong> Keys
        {
            get
            {
                for (uint i = 0; i < _values.Count; ++i)
                {
                    yield return ObservationBeginTick + i;
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
                        $"becouse observation begin at {ObservationBeginTick} tick."
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
                
                return _values[(int)checked(ticks - ObservationBeginTick)];
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
            return this[ticks];
        }

        public bool ContainsKey(ulong key)
        {
            return ObservationBeginTick <= key && key <= LastObservedTick;
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
                var key = ObservationBeginTick + i;
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
