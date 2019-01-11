using System;

namespace Mechanix.Laws
{
    /// <summary>
    /// Factory for forces obeyed by the gravity law
    /// </summary>
    public static class GravityLaw
    {
        /// <summary>
        /// Get force value from these parameters
        /// </summary>
        public static Force Eval(in PointMass objectEntity, in PointMass subjectEntity, double gravityCoefficient)
        {
            double dx2 = (objectEntity.X.Position - subjectEntity.X.Position) * (objectEntity.X.Position - subjectEntity.X.Position);
            double dy2 = (objectEntity.Y.Position - subjectEntity.Y.Position) * (objectEntity.Y.Position - subjectEntity.Y.Position);
            double dz2 = (objectEntity.Z.Position - subjectEntity.Z.Position) * (objectEntity.Z.Position - subjectEntity.Z.Position);

            double distance = Math.Sqrt(dx2 + dy2 + dz2);
            double forceValue = gravityCoefficient * objectEntity.Mass * subjectEntity.Mass / distance / distance;

            return new Force
            (
                -(objectEntity.X.Position - subjectEntity.X.Position) / distance * forceValue,
                -(objectEntity.Y.Position - subjectEntity.Y.Position) / distance * forceValue,
                -(objectEntity.Z.Position - subjectEntity.Z.Position) / distance * forceValue
            );
        }
        /// <summary>
        /// Get gravity force evaluation law from these params
        /// </summary>
        public static Func<PhysicalContext<TEntityKey>, Force> GetLaw<TEntityKey>
        (
            TEntityKey objectEntityKey,
            TEntityKey subjectEntityKey,
            double gravityCoefficient
        )
        {
            return (context) => Eval(context[objectEntityKey], context[subjectEntityKey], gravityCoefficient);
        }
    }
}
