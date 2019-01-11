using System;
using System.Collections.Generic;
using System.Text;

namespace Mechanix
{
    /// <summary>
    /// Contains value, that may change during observable context evolution
    /// </summary>
    /// <typeparam name="TEntityKey"> Type of entity keys in observable context </typeparam>
    /// <typeparam name="TValue"> Type of stored value </typeparam>
    public sealed class ContextDependentValue<TEntityKey, TValue> : ContextObserver<TEntityKey> where TValue : struct
    {
        public TValue Value { get; private set; }
        public ulong LastValueChangeTick { get; private set; }
        public double LastValueChangeTime => LastValueChangeTick * ObservableContext.TimePerTick;

        Func<PhysicalContext<TEntityKey>, TValue, TValue?> _newValueFunc;

        /// <param name="newValueFunc">
        /// Function for evaluating new <see cref="Value"/> through new observable context state and old value.
        /// If result is <see langword="null"/> <see cref="Value"/> doesn't changes
        /// </param>
        public ContextDependentValue 
        (
            TValue startValue,
            PhysicalContext<TEntityKey> observableContext, 
            Func<PhysicalContext<TEntityKey>, TValue, TValue?> newValueFunc
        )
        : base(observableContext)
        {
            _newValueFunc = newValueFunc;
            LastValueChangeTick = ObservationBeginTick;
            Value = startValue;
        }

        protected override void Observe()
        {
            var result = _newValueFunc(ObservableContext, Value);
            if (result != null)
            {
                Value = (TValue)result;
                LastValueChangeTick = ObservableContext.Ticks;
            }
        }
    }
}
