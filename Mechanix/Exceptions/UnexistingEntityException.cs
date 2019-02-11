using System;

namespace Mechanix
{
    /// <summary>
    /// Throws when unexisting entity in a <see cref="PhysicalContext{TEntityKey}"/> is tried to be accessed
    /// </summary>
    /// <typeparam name="TEntityKey"> Type of keys </typeparam>
    public class UnexistingEntityException<TEntityKey> : Exception
    {
        /// <summary>
        /// <see cref="PhysicalContext{TEntityKey}"/> that invoke this exception
        /// </summary>
        public PhysicalContext<TEntityKey> Context { get; }

        /// <summary>
        /// Key that points to unexisting entity;
        /// </summary>
        public TEntityKey EntityKey { get; }

        internal UnexistingEntityException(PhysicalContext<TEntityKey> context, TEntityKey key)
            : base($"Physical context doesn't contain entity with key {key}.") 
        {
            Context = context;
            EntityKey = key;
        }
    }
}
