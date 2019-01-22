using System;
using System.Collections.Generic;
using System.Linq;

namespace Mechanix
{
    /// <summary>
    /// Represents self-disposing <see cref="ContextObserver{TEntityKey}"/> 
    /// that tracks context current tick number 
    /// </summary>
    /// <typeparam name="TEntityKey"></typeparam>
    public sealed class ContextProgressTracker<TEntityKey> : ContextObserver<TEntityKey>
    {
        readonly ulong[] _checkPoints;
        int _currentCheckPointIndex;
        public IEnumerable<ulong> CheckPoints => _checkPoints??Enumerable.Empty<ulong>();

        /// <summary>
        /// Goal tick
        /// </summary>
        public ulong Goal { get; }

        /// <summary>
        /// Number in range [0; 1] that represent progress in reaching <see cref="Goal"/>
        /// </summary>
        public double Progress => ((double)(LastObservedTick - ObservationBeginTick)) / ((double)(Goal - ObservationBeginTick));

        /// <summary>
        /// Raises when observable context reaches <see cref="Goal"/>
        /// </summary>
        public event EventHandler<EventArgs> OnGoal;

        /// <summary>
        /// Raises when observable context reaches one of <see cref="CheckPoints"/>>
        /// </summary>
        public event EventHandler<EventArgs> OnCheckPoint;

        public ContextProgressTracker
        (
            PhysicalContext<TEntityKey> observableContext,
            ulong goalTick,
            params ulong[] checkPoints
        )
        : base(observableContext)
        {
            if (checkPoints.Any(point => observableContext.Ticks > point || point > goalTick))
            {
                throw new ArgumentOutOfRangeException
                (
                    $"Can't create ContextProgressTracker with checkpoints {CheckPoints}" +
                    $"because one or more of checkpoints are already less than observable" +
                    $"context current tick ({observableContext.Ticks}) or bigger than goal tick ({goalTick})"
                );
            }
            _checkPoints = checkPoints;
            Array.Sort(_checkPoints);
            _currentCheckPointIndex = 0;
            Goal = goalTick;
            Observe();
        }

        public static ContextProgressTracker<TEntityKey> FromTime
        (
            PhysicalContext<TEntityKey> observableContext,
            double goalTime,
            params double[] checkPoints
        )        
        {
            return new ContextProgressTracker<TEntityKey>
            (
                observableContext,
                (ulong)Math.Round(goalTime / observableContext.TimePerTick),
                (from c in checkPoints select (ulong)(Math.Round(c / observableContext.TimePerTick))).ToArray()
            );
        }
        

        protected override void Observe()
        {
            var tick = ObservableContext.Ticks;
            if (_checkPoints != null)
            {
                while (_currentCheckPointIndex < _checkPoints.Length && tick == _checkPoints[_currentCheckPointIndex])
                {
                    OnCheckPoint?.Invoke(this, EventArgs.Empty);
                    ++_currentCheckPointIndex;
                }
            }
            if (tick == Goal)
            {
                OnGoal?.Invoke(this, EventArgs.Empty);
                Dispose();
            }
        }
    }
}
