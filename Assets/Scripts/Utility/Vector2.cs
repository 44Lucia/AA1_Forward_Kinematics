using System;

namespace Utility
{
    public struct Vector2
    {
        public float x, y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 normalized
        {
            get
            {
                Vector2 result = new Vector2(x, y);
                result.Normalize();
                return result;
            }
        }

        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y);
            }
        }

        public void Normalize()
        {
            float mag = magnitude;
            if (mag > 1E-05)
            {
                x /= mag;
                y /= mag;
            }
            else
            {
                x = 0;
                y = 0;
            }
        }

        #region Operators

        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, Vector2 b) => new(a.x * b.x, a.y * b.y);
        public static Vector2 operator /(Vector2 a, Vector2 b) => new(a.x / b.x, a.y / b.y);
        public static Vector2 operator -(Vector2 a) => new(0f - a.x, 0f - a.y);
        public static Vector2 operator *(Vector2 a, float d) => new(a.x * d, a.y * d);
        public static Vector2 operator *(float d, Vector2 a) => new(a.x * d, a.y * d);
        public static Vector2 operator /(Vector2 a, float d) => new(a.x / d, a.y / d);

        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            float num = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            return num * num + num2 * num2 < 9.99999944E-11f;
        }

        public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

        #endregion
    }
}