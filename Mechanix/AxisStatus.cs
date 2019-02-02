using System;

namespace Mechanix
{
    /// <summary>
    /// Represent a position, a velocity and an acceleration of an entity within one axis
    /// </summary>
    [Serializable]
    public readonly struct AxisStatus : IEquatable<AxisStatus>
    {
        public double Position { get; }
        public double Velocity { get; }

        public AxisStatus(double position, double velocity = 0)
        {
            Position = position;
            Velocity = velocity;
        }

        public override bool Equals(object obj)
        {
            return obj is AxisStatus && Equals((AxisStatus)obj);
        }

        public bool Equals(AxisStatus other)
        {
            return Position == other.Position &&
                   Velocity == other.Velocity;
        }

        public override int GetHashCode()
        {
            var hashCode = 1546832041;
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Velocity.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"[pos: {Position}; v: {Velocity}]";
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
