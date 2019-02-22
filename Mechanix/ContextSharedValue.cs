using System;
using System.Threading;

namespace Mechanix
{
    /// <summary>
    /// Represents cacheable, lazy-evaluated value, that depends on <see cref="Context"/> state
    /// </summary>
    public class ContextSharedValue<TEntityKey, TValue>
    {
        TValue _value;
        ulong _tick;
        readonly Func<PhysicalContext<TEntityKey>, TValue> _valueFunc;

        readonly object _lockObject = new object();

        public PhysicalContext<TEntityKey> Context { get; }
        public TValue Value
        {
            get
            {
                if (Context.Ticks == _tick) return _value;

                lock (_lockObject)
                {
                    _value = _valueFunc(Context);
                    _tick = Context.Ticks;
                }
                return _value;
            }
        }

        public ContextSharedValue
        (
            PhysicalContext<TEntityKey> context, 
            Func<PhysicalContext<TEntityKey>, TValue> valueFunc
        )
        {
            Context = context;
            _valueFunc = valueFunc;
            _value = _valueFunc(context);
            _tick = context.Ticks;
        }
    }
}
