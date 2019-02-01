using System;

namespace Mechanix
{
    /// <summary>
    /// Represent a physic force value within the three dimensional world
    /// </summary>
    [Serializable]
    public readonly struct Force : IEquatable<Force>
    {
        public static Force Zero => new Force(0, 0, 0);

        public double XComponent { get; }
        public double YComponent { get; }
        public double ZComponent { get; }

        /// <summary>
        /// Full value of Force within the three dimensional world
        /// </summary>
        public double Value
        {
            get
            {
                double xsq = XComponent * XComponent;
                double ysq = YComponent * YComponent;
                double zsq = ZComponent * ZComponent;
                return Math.Sqrt(xsq + ysq + zsq);
            }
        }

        public Force(double xComponent, double yComponent, double zComponent)
        {
            XComponent = xComponent;
            YComponent = yComponent;
            ZComponent = zComponent;
        }

        public Force Substract(in Force force)
        {
            return new Force
            (
                XComponent - force.XComponent,
                YComponent - force.YComponent,
                ZComponent - force.ZComponent
            );
        }

        public Force Add(in Force force)
        {
            return new Force
            (
                XComponent + force.XComponent,
                YComponent + force.YComponent,
                ZComponent + force.ZComponent
            );
        }

        public Force Multiply(double number)
        {
            return new Force
            (
                XComponent * number,
                YComponent * number,
                ZComponent * number
            );
        }

        public override bool Equals(object obj)
        {
            return obj is Force && Equals((Force)obj);
        }

        public bool Equals(Force other)
        {
            return XComponent == other.XComponent &&
                   YComponent == other.YComponent &&
                   ZComponent == other.ZComponent &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = -1741574105;
            hashCode = hashCode * -1521134295 + XComponent.GetHashCode();
            hashCode = hashCode * -1521134295 + YComponent.GetHashCode();
            hashCode = hashCode * -1521134295 + ZComponent.GetHashCode();
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"Force[x: {XComponent}; y: {YComponent}; z: {ZComponent}";
        }

        public static bool operator ==(Force force1, Force force2)
        {
            return force1.Equals(force2);
        }

        public static bool operator !=(Force force1, Force force2)
        {
            return !(force1 == force2);
        }

        public static Force operator -(in Force force)
        {
            return force.Multiply(-1);
        }

        public static Force operator -(in Force force1, in Force force2)
        {
            return force1.Substract(force2);
        }

        public static Force operator +(in Force force1, in Force force2)
        {
            return force1.Add(force2);
        }

        public static Force operator *(in Force force1, double number)
        {
            return force1.Multiply(number);
        }
    }
}