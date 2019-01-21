using System;
using System.Collections.Generic;

namespace Mechanix
{
    /// <summary>
    /// Represents a point mass entity, that is obeyed by the second law of Newton
    /// </summary>
    public readonly struct PointMass : IEquatable<PointMass>
    {
        readonly AxisStatus _x, _y, _z;
        public AxisStatus X => _x;
        public AxisStatus Y => _y;
        public AxisStatus Z => _z;

        readonly double _mass;
        public double Mass => _mass;

        public double Velocity
        {
            get => Math.Sqrt(_x.Velocity * _x.Velocity + _y.Velocity * _y.Velocity + _z.Velocity * _z.Velocity);
        }
        public double Acceleration
        {
            get => Math.Sqrt(_x.Acceleration * _x.Acceleration + _y.Acceleration * _y.Acceleration + _z.Acceleration * _z.Acceleration);
        }

        public PointMass(in AxisStatus x, in AxisStatus y, in AxisStatus z, double mass)
        {
            _x = x;
            _y = y;
            _z = z;
            _mass = mass;
        }

        public PointMass(double xPosition, double yPosition, double zPosition, double mass)
        {
            _x = new AxisStatus(xPosition, 0, 0);
            _y = new AxisStatus(yPosition, 0, 0);
            _z = new AxisStatus(zPosition, 0, 0);
            _mass = mass;
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
                new AxisStatus(_x.Position + _x.Velocity * dt, _x.Velocity + _x.Acceleration * dt, nextAX),
                new AxisStatus(_y.Position + _y.Velocity * dt, _y.Velocity + _y.Acceleration * dt, nextAY),
                new AxisStatus(_z.Position + _z.Velocity * dt, _z.Velocity + _z.Acceleration * dt, nextAZ),
                Mass
            );
        }

        public override bool Equals(object obj)
        {
            return obj is PointMass && Equals((PointMass)obj);
        }

        public bool Equals(PointMass other)
        {
            return X.Equals(other._x) &&
                   Y.Equals(other._y) &&
                   Z.Equals(other._z) &&
                   Mass == other.Mass;
        }

        public override int GetHashCode()
        {
            var hashCode = -1636101175;
            hashCode = hashCode * -1521134295 + EqualityComparer<AxisStatus>.Default.GetHashCode(_x);
            hashCode = hashCode * -1521134295 + EqualityComparer<AxisStatus>.Default.GetHashCode(_y);
            hashCode = hashCode * -1521134295 + EqualityComparer<AxisStatus>.Default.GetHashCode(_z);
            hashCode = hashCode * -1521134295 + Mass.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"PointMass[x: {_x}; y: {_y}; z: {_z}; Mass = {_mass}]";
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
