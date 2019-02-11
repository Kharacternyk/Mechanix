using System;

namespace Mechanix
{
    /// <summary>
    /// Throws if entity adding requested when
    /// <see cref="PhysicalContext{TEntityKey}.Count"/> is already equal
    /// <see cref="PhysicalContext{TEntityKey}.Capacity"/>
    /// </summary>
    public class FilledPhysicalContextException<TEntityKey> : Exception
    {
        /// <summary>
        /// <see cref="PhysicalContext{TEntityKey}"/> that invoke this exception
        /// </summary>
        public PhysicalContext<TEntityKey> Context { get; }

        internal FilledPhysicalContextException(PhysicalContext<TEntityKey> context)
            : base($"Physical context is already filled. Can't add entities anymore.")
        {
            Context = context;
        }
    }
}
