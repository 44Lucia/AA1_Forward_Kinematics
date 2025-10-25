namespace Utility
{
    public struct Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

        public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator -(Vector3 a)            => new(-a.x, -a.y, -a.z);
        public static Vector3 operator *(Vector3 a, float s)   => new(a.x * s, a.y * s, a.z * s);
        public static Vector3 operator *(float s, Vector3 a)   => a * s;
        public static Vector3 operator /(Vector3 a, float s)   => new(a.x / s, a.y / s, a.z / s);

        public static Vector3 Zero => new(0,0,0);
        public static float   Dot(Vector3 a, Vector3 b)   => a.x*b.x + a.y*b.y + a.z*b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new(
            a.y*b.z - a.z*b.y,
            a.z*b.x - a.x*b.z,
            a.x*b.y - a.y*b.x
        );
        public float  SqrMagnitude() => x*x + y*y + z*z;
        public float  Magnitude()    => (float)System.Math.Sqrt(SqrMagnitude());
        public Vector3 Normalized()  { var m = Magnitude(); return m > 1e-8f ? this / m : Zero; }
        public static float   Distance(Vector3 a, Vector3 b) => (a - b).Magnitude();
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * t;

        public override string ToString() => $"({x:F3},{y:F3},{z:F3})";
        public override int GetHashCode() => (x,y,z).GetHashCode();
        public override bool Equals(object o) => o is Vector3 v && (this - v).SqrMagnitude() < 1e-10f;
        public static bool operator ==(Vector3 a, Vector3 b) => (a - b).SqrMagnitude() < 1e-10f;
        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
    }
}
