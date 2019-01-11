using System;

namespace Mechanix.Laws
{
    /// <summary>
    /// Factory for forces obeyed by the Stokes' law: fluid resistance proportional to velocity
    /// </summary>
    public static class StokesDragLaw
    {
        /// <summary>
        /// Get force value from these parameters
        /// </summary>
        public static Force Eval(in PointMass objectEntity, double resistanceCoefficient)
        {
            return new Force
            (
                -objectEntity.X.Velocity * resistanceCoefficient,
                -objectEntity.Y.Velocity * resistanceCoefficient,
                -objectEntity.Z.Velocity * resistanceCoefficient
            );
        }

        /// <summary>
        /// Get Stokes' force evaluation law from these params
        /// </summary>
        public static Func<PhysicalContext<TEntityKey>, Force> GetLaw<TEntityKey>(TEntityKey objectEntityKey, double resistanceCoefficient)
        {
            return (context) => Eval(context[objectEntityKey], resistanceCoefficient); 
        }
    }
}
