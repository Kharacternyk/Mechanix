using System;

namespace Mechanix
{
    /// <summary>
    /// Set of flags that determines how 
    /// the instance of <see cref="PhysicalContext{TEntityKey}"/> process entities
    /// </summary>
    [Flags]
    public enum SimulationParams
    {
        None =                0b_00000,
        /// <summary>
        /// Use parallel loop to processing next state of entities
        /// <para></para>
        /// NOTE: Exceptions during evaluation of force values will 
        /// be wrapped into the <see cref="AggregateException"/> instance
        /// </summary>
        ParallelEntities =    0b_00001,
        /// <summary>
        /// Use parallel loop to invoke <see cref="PhysicalContext{TEntityKey}.OnTick"/> subscribers
        /// <para></para>
        /// NOTE: Exceptions during invocation of subscribers will
        /// be wrapped into the <see cref="AggregateException"/> instance
        /// </summary>
        ParallelSubscribers = 0b_00010
    }
}
