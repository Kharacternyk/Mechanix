using System;
using System.Collections.Generic;
using System.Text;

namespace Mechanix.Laws
{
    /// <summary>
    /// Factory for forces obeyed by the drag law: fluid resistance proportional to square of velocity
    /// </summary>
    public static class DragLaw
    {
        /// <summary>
        /// Get force value from these parameters
        /// </summary>
        public static Force Eval(in PointMass objectEntity, double resistanceCoefficient)
        {
            return new Force
            (
                -objectEntity.X.Velocity * objectEntity.Velocity * resistanceCoefficient,
                -objectEntity.Y.Velocity * objectEntity.Velocity * resistanceCoefficient,
                -objectEntity.Z.Velocity * objectEntity.Velocity *resistanceCoefficient
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
