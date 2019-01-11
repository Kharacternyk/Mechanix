using System;
using System.Collections.Generic;

namespace Mechanix
{
    /// <summary>
    /// Represents a point mass entity, that is obeyed by the second law of Newton
    /// </summary>
    public readonly struct PointMass : IEquatable<PointMass>
    {
        //c# 7.3 don't accept 
        //public ref readonly AxisStatus X => ref _x;

        public AxisStatus X { get; }
        public AxisStatus Y { get; } 
        public AxisStatus Z { get; }
        public double Mass { get; }

        public double Velocity
        {
            get => Math.Sqrt(X.Velocity * X.Velocity + Y.Velocity * Y.Velocity + Z.Velocity * Z.Velocity);
        }
        public double Acceleration
        {
            get => Math.Sqrt(X.Acceleration * X.Acceleration + Y.Acceleration * Y.Acceleration + Z.Acceleration * Z.Acceleration);
        }

        public PointMass(in AxisStatus x, in AxisStatus y, in AxisStatus z, double mass)
        {
            X = x;
            Y = y;
            Z = z;
            Mass = mass;
        }

        public PointMass(double xPosition, double yPosition, double zPosition, double mass)
        {
            X = new AxisStatus(xPosition, 0, 0);
            Y = new AxisStatus(yPosition, 0, 0);
            Z = new AxisStatus(zPosition, 0, 0);
            Mass = mass;
        }

        /// <summary>
        /// Returns <see cref="PointMass"/> entity, 
        /// that represents this entity after a period of time
        ///  <paramref name="dt"/> and forces <paramref name="forcesAttached"/>.
        /// </summary>
        /// <param name="dt">
        /// A period of time, during that all components are considered
        /// to be uniform
        /// </param>
        /// <param name="forcesAttached">
        /// Set of <see cref="Force"/> values, that have impact on this entity
        /// </param>
        /// <returns></returns>
        public PointMass Next(double dt, params Force[] forcesAttached)
        {
            double nextAX = 0, nextAY = 0, nextAZ = 0;
            for (int i = 0; i < forcesAttached.Length; ++i)
            {
                nextAX += forcesAttached[i].XComponent;
                nextAY += forcesAttached[i].YComponent;
                nextAZ += forcesAttached[i].ZComponent;            
            }

            nextAX /= Mass;
            nextAY /= Mass;
            nextAZ /= Mass;

            return new PointMass
            (
                new AxisStatus(X.Position + X.Velocity * dt, X.Velocity + X.Acceleration * dt, nextAX),
                new AxisStatus(Y.Position + Y.Velocity * dt, Y.Velocity + Y.Acceleration * dt, nextAY),
                new AxisStatus(Z.Position + Z.Velocity * dt, Z.Velocity + Z.Acceleration * dt, nextAZ),
                Mass
            );
        }

        public override bool Equals(object obj)
        {
            return obj is PointMass && Equals((PointMass)obj);
        }

        public bool Equals(PointMass other)
        {
            return X.Equals(other.X) &&
                   Y.Equals(other.Y) &&
                   Z.Equals(other.Z) &&
                   Mass == other.Mass;
        }

        public override int GetHashCode()
        {
            var hashCode = -1636101175;
            hashCode = hashCode * -1521134295 + EqualityComparer<AxisStatus>.Default.GetHashCode(X);
            hashCode = hashCode * -1521134295 + EqualityComparer<AxisStatus>.Default.GetHashCode(Y);
            hashCode = hashCode * -1521134295 + EqualityComparer<AxisStatus>.Default.GetHashCode(Z);
            hashCode = hashCode * -1521134295 + Mass.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"PointMass[x: {X}; y: {Y}; z: {Z}; Mass = {Mass}]";
        }

        public static bool operator ==(PointMass mass1, PointMass mass2)
        {
            return mass1.Equals(mass2);
        }

        public static bool operator !=(PointMass mass1, PointMass mass2)
        {
            return !(mass1 == mass2);
        }
    }
}
