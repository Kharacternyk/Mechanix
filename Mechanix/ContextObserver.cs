using System;

namespace Mechanix
{
    /// <summary>
    /// Represent <see cref="PhysicalContext{TEntityKey}"/> observer.
    /// <para>
    /// Does dome actions each time the <see cref="ObservableContext"/>'s method <see cref="PhysicalContext{TEntityKey}.Tick()"/>
    /// is called until is disposed
    /// </para>
    /// </summary>
    /// <typeparam name="TEntityKey"></typeparam>
    public abstract class ContextObserver<TEntityKey> : IDisposable
    {
        public ulong ObservationBeginTick { get; }
        public double ObservationBeginTime => ObservationBeginTick * ObservableContext.TimePerTick;

        public ulong LastObservedTick { get; private set; }
        public double LastObservationTime => LastObservedTick * ObservableContext.TimePerTick;

        public PhysicalContext<TEntityKey> ObservableContext { get; }

        /// <summary>
        /// <see langword="false"/>, if this <see cref="ContextTracker{TEntityKey, TValue}"/> 
        /// doesn't observe <see cref="PhysicalContext{TEntityKey}"/> anymore
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Unsubscribe from <see cref="ObservableContext"/>
        /// </summary>
        public void Dispose()
        {
            ObservableContext.OnTick -= OnTick;
            IsActive = false;
            OnDisposed();
        }

        public ContextObserver(PhysicalContext<TEntityKey> observableContext)
        {
            LastObservedTick = ObservationBeginTick = observableContext.Ticks;
            ObservableContext = observableContext;
            IsActive = true;
            ObservableContext.OnTick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Observe();
            ++LastObservedTick;
        }

        /// <summary>
        /// Method for handling new state of <see cref="ObservableContext"/>
        /// </summary>
        protected abstract void Observe();

        /// <summary>
        /// Additional logic of disposing
        /// </summary>
        protected virtual void OnDisposed() { }
    }
}
