namespace Utility
{
    public struct Quaternion
    {
        public float x, y, z, w;

        public Quaternion(float x, float y, float z, float w)
        { this.x = x; this.y = y; this.z = z; this.w = w; }

        public static Quaternion Identity => new(0,0,0,1);

        // Hamilton (a * b)
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.w*b.x + a.x*b.w + a.y*b.z - a.z*b.y,
                a.w*b.y - a.x*b.z + a.y*b.w + a.z*b.x,
                a.w*b.z + a.x*b.y - a.y*b.x + a.z*b.w,
                a.w*b.w - a.x*b.x - a.y*b.y - a.z*b.z
            );
        }

        public Quaternion Conjugate() => new(-x, -y, -z, w);

        public Quaternion Normalized()
        {
            float n = (float)System.Math.Sqrt(x*x + y*y + z*z + w*w);
            if (n < 1e-8f) return Identity;
            return new Quaternion(x/n, y/n, z/n, w/n);
        }

        // q * (0,v) * q*
        public Utility.Vector3 Rotate(Utility.Vector3 v)
        {
            var qv = new Quaternion(v.x, v.y, v.z, 0f);
            var res = this * qv * this.Conjugate();
            return new Utility.Vector3(res.x, res.y, res.z);
        }

        public static Quaternion AxisAngle(Utility.Vector3 axis, float angleRad)
        {
            float m = axis.Magnitude();
            if (m < 1e-8f) return Identity;
            float nx = axis.x / m, ny = axis.y / m, nz = axis.z / m;
            float half = angleRad * 0.5f;
            float s = (float)System.Math.Sin(half);
            float c = (float)System.Math.Cos(half);
            return new Quaternion(nx*s, ny*s, nz*s, c);
        }
    }
}
