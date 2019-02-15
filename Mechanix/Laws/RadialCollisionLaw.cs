using System;

namespace Mechanix.Laws
{
    /// <summary>
    /// Factory for forces, that occur when one entity is too close to other entity.
    /// Proportional to deformation
    /// </summary>
    public static class RadialCollisionLaw
    {
        /// <summary>
        /// Get force value from these parameters
        /// </summary>
        /// <param name="criticalDistance">
        /// If distance between entities is smaller than this, than force occurred 
        /// </param>
        public static Force Eval(in PointMass objectEntity, in PointMass subjectEntity, double criticalDistance, double elasticityCoefficient)
        {
            double dx2 = objectEntity.X.Position - subjectEntity.X.Position;
            if (Math.Abs(dx2) > criticalDistance) return Force.Zero;
            dx2 *= dx2;

            double dy2 = objectEntity.Y.Position - subjectEntity.Y.Position;
            if (Math.Abs(dy2) > criticalDistance) return Force.Zero;
            dy2 *= dy2;

            double dz2 = objectEntity.Z.Position - subjectEntity.Z.Position;
            if (Math.Abs(dz2) > criticalDistance) return Force.Zero;
            dz2 *= dz2;

            double distance = Math.Sqrt(dx2 + dy2 + dz2);
            if (distance > criticalDistance) return Force.Zero;

            double deformation = criticalDistance - distance;
            double forceValue = deformation * elasticityCoefficient;

            return new Force
            (
                (objectEntity.X.Position - subjectEntity.X.Position) / distance * forceValue,
                (objectEntity.Y.Position - subjectEntity.Y.Position) / distance * forceValue,
                (objectEntity.Z.Position - subjectEntity.Z.Position) / distance * forceValue
            );
        }

        public static Func<PhysicalContext<TEntityKey>, Force> GetLaw<TEntityKey>
        (
            TEntityKey objectEntityKey,
            TEntityKey subjectEntityKey,
            double criticalDistance, 
            double elasticityCoefficient
        )
        {
            return (context) => Eval(context[objectEntityKey], context[subjectEntityKey], criticalDistance, elasticityCoefficient);
        }
    }
}
