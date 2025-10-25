using UnityEngine;                  
using UVec3  = Utility.Vector3;    
using UQuat  = Utility.Quaternion;
using UEVec3 = UnityEngine.Vector3; 
using UEQuat = UnityEngine.Quaternion;

namespace Scripts
{
    static class UBridge
    {
        public static UEVec3 ToU(UVec3 v)  => new UEVec3(v.x, v.y, v.z);
        public static UEQuat ToU(UQuat q)  => new UEQuat(q.x, q.y, q.z, q.w);
    }

    public class ExcavatorDrive : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float linearSpeed = 2.5f;    
        [SerializeField] private float angularSpeedDeg = 90f;
        [SerializeField] private float heightOffset = 0.0f;

        [Header("Suelo (snap opcional)")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private bool snapToGround = true;
        [SerializeField] private float rayLength = 5f;

        private UVec3 pos;
        private UQuat rot;

        float SafeAxis(string n) { try { return Input.GetAxis(n); } catch { return 0f; } }

        void Start()
        {
            UEVec3 p = transform.position;
            pos = new UVec3(p.x, p.y, p.z);

            UEQuat rq = transform.rotation;
            rot = new UQuat(rq.x, rq.y, rq.z, rq.w).Normalized();
        }

        void Update()
        {
            float dt = Time.deltaTime;

            float fwd = 0f, turn = 0f;
            if (Input.GetKey(KeyCode.W)) fwd += 1f;
            if (Input.GetKey(KeyCode.S)) fwd -= 1f;
            if (Input.GetKey(KeyCode.D)) turn += 1f;
            if (Input.GetKey(KeyCode.A)) turn -= 1f;

            fwd  += SafeAxis("JoyLY"); // ↑ = +1
            turn += SafeAxis("JoyLX"); // → = +1

            if (fwd  > 1f) fwd = 1f; else if (fwd  < -1f) fwd  = -1f;
            if (turn > 1f) turn = 1f; else if (turn < -1f) turn = -1f;

            float angRad = turn * Utility.MathLite.Deg2Rad(angularSpeedDeg) * dt;
            if (System.Math.Abs(angRad) > 1e-8f)
                rot = (rot * UQuat.AxisAngle(new UVec3(0,1,0), angRad)).Normalized();

            UVec3 localForward = new UVec3(0,0,1);
            UVec3 worldForward = rot.Rotate(localForward);
            pos = pos + worldForward * (linearSpeed * fwd * dt);

            if (snapToGround)
            {
                UEVec3 origin = UBridge.ToU(pos) + UEVec3.up * 0.5f;
                if (Physics.Raycast(origin, UEVec3.down, out RaycastHit hit, rayLength, groundMask))
                    pos = new UVec3(hit.point.x, hit.point.y + heightOffset, hit.point.z);
            }

            transform.SetPositionAndRotation(UBridge.ToU(pos), UBridge.ToU(rot));
        }
    }
}
