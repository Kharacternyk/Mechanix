using System;

namespace Mechanix
{
    /// <summary>
    /// Represent a position, a velocity and an acceleration of an entity within one axis
    /// </summary>
    public readonly struct AxisStatus : IEquatable<AxisStatus>
    {
        public double Position { get; }
        public double Velocity { get; }
        public double Acceleration { get; }

        public AxisStatus(double position, double velocity = 0, double acceleration = 0)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = acceleration;
        }

        public override bool Equals(object obj)
        {
            return obj is AxisStatus && Equals((AxisStatus)obj);
        }

        public bool Equals(AxisStatus other)
        {
            return Position == other.Position &&
                   Velocity == other.Velocity &&
                   Acceleration == other.Acceleration;
        }

        public override int GetHashCode()
        {
            var hashCode = 1546832041;
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Velocity.GetHashCode();
            hashCode = hashCode * -1521134295 + Acceleration.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"[pos: {Position}; v: {Velocity}; a: {Acceleration}]";
        }

        public static bool operator ==(AxisStatus status1, AxisStatus status2)
        {
            return status1.Equals(status2);
        }

        public static bool operator !=(AxisStatus status1, AxisStatus status2)
        {
            return !(status1 == status2);
        }
    }
}
