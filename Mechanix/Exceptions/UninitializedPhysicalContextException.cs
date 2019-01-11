using System;
using System.Collections.Generic;
using System.Text;

namespace Mechanix
{
    /// <summary>
    /// Throws when <see cref="PhysicalContext{TEntityKey}.Tick(bool)"/>
    /// is called while entities count is lesser then capacity 
    /// </summary>
    public class UninitializedPhysicalContextException<TEntityKey> : Exception
    {
        /// <summary>
        /// <see cref="PhysicalContext{TEntityKey}"/> that invoke this exception
        /// </summary>
        public PhysicalContext<TEntityKey> Context { get; }

        internal UninitializedPhysicalContextException(PhysicalContext<TEntityKey> context)
            : base($"Access to Tick() method is denied until context will be filled")
        {
            Context = context;
        }
    }
}
