using System;
using System.Collections.Generic;
using System.Text;

namespace Mechanix
{
    /// <summary>
    /// Represents <see cref="ContextObserver{TEntityKey}"/> 
    /// that tracks context current tick number 
    /// </summary>
    /// <typeparam name="TEntityKey"></typeparam>
    public sealed class ContextProgressTracker<TEntityKey> : ContextObserver<TEntityKey>
    {
        /// <summary>
        /// Goal tick
        /// </summary>
        public ulong Goal { get; }
        /// <summary>
        /// Number in range [0; 1] that represent progress in reaching <see cref="this.Goal"/>
        /// </summary>
        public double Progress => (LastObservedTick - ObservationBeginTick) / (Goal - ObservationBeginTick);

        /// <summary>
        /// Raises when observable context reaches <see cref="Goal"/>
        /// </summary>
        public event EventHandler<EventArgs> OnGoal;

        public ContextProgressTracker
        (
            PhysicalContext<TEntityKey> observableContext,
            ulong goalTick
        )
        : base(observableContext)
        {
            Goal = goalTick;
        }

        protected override void Observe()
        {
            if (ObservableContext.Ticks == Goal)
            {
                OnGoal?.Invoke(this, EventArgs.Empty);
                Dispose();
            }
        }
    }
}
