using System;

namespace Mechanix.Laws
{
    /// <summary>
    /// Factory for forces obeyed by the Hooke's law: the force value is proportional to the extension of distance
    /// </summary>
    public static class HookesLaw
    {
        /// <summary>
        /// Get force value from these parameters
        /// </summary>
        public static Force Eval(in PointMass objectEntity, in PointMass subjectEntity, double undeformedDistance, double elasticityCoefficient)
        {
            double dx2 = (objectEntity.X.Position - subjectEntity.X.Position) * (objectEntity.X.Position - subjectEntity.X.Position);
            double dy2 = (objectEntity.Y.Position - subjectEntity.Y.Position) * (objectEntity.Y.Position - subjectEntity.Y.Position);
            double dz2 = (objectEntity.Z.Position - subjectEntity.Z.Position) * (objectEntity.Z.Position - subjectEntity.Z.Position);

            double distance = Math.Sqrt(dx2 + dy2 + dz2);
            double forceValue = (distance - undeformedDistance) * elasticityCoefficient;

            return new Force
            (
                -(objectEntity.X.Position - subjectEntity.X.Position) / distance * forceValue,
                -(objectEntity.Y.Position - subjectEntity.Y.Position) / distance * forceValue,
                -(objectEntity.Z.Position - subjectEntity.Z.Position) / distance * forceValue
            );
        }

        /// <summary>
        /// Get Hooke's force evaluation law from these params
        /// </summary>
        public static Func<PhysicalContext<TEntityKey>, Force> GetLaw<TEntityKey>
        (
            TEntityKey objectEntityKey, 
            TEntityKey subjectEntityKey, 
            double undeformedDistance, 
            double elasticityCoefficient
        )
        {
            return (context) => Eval(context[objectEntityKey], context[subjectEntityKey], undeformedDistance, elasticityCoefficient);
        }
    }
}
