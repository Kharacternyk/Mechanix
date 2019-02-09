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
        ParallelEntities =    0b_00001,
        ParallelSubscribers = 0b_00010
    }
}
