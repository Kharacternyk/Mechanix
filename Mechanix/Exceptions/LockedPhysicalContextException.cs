using System;

namespace Mechanix
{
    /// <summary>
    /// Throws when change of <see cref="PhysicalContext{TEntityKey}"/> 
    /// state is requsted while <see cref="PhysicalContext{TEntityKey}.Tick()"/> is executed
    /// </summary>
    /// <typeparam name="TEntityKey"> Type of keys </typeparam>
    public class LockedPhysicalContextException<TEntityKey> : Exception
    {
        /// <summary>
        /// <see cref="PhysicalContext{TEntityKey}"/> that invoke this exception
        /// </summary>
        public PhysicalContext<TEntityKey> Context { get; }

        internal LockedPhysicalContextException(PhysicalContext<TEntityKey> context)
            : base("Access to PhysicalContext.Tick() method are illegal while PhysicalContext.Tick() method is executed.")
        {
            Context = context;
        }
    }
}
