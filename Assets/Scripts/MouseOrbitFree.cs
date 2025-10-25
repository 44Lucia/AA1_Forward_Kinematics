using UnityEngine;
using UVec3  = Utility.Vector3;    
using UQuat  = Utility.Quaternion;
using UEVec3 = UnityEngine.Vector3;  
using UEQuat = UnityEngine.Quaternion;

namespace Scripts
{
    public class MouseOrbitFree : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;       
        [SerializeField] private UVec3 targetOffset = new UVec3(0f, 1.4f, 0f); 

        [Header("Órbita")]
        [SerializeField] private float minPitchDeg = -35f;
        [SerializeField] private float maxPitchDeg =  70f;
        [SerializeField] private float yawSpeed    = 180f;  
        [SerializeField] private float pitchSpeed  = 120f;  
        [SerializeField] private float zoomMin     = 2.5f;
        [SerializeField] private float zoomMax     = 10f;
        [SerializeField] private float zoomSpeed   = 6f;   

        [Header("Suavizado")]
        [SerializeField] private float followLerp  = 12f; 

        [Header("Evitar clipping (opcional)")]
        [SerializeField] private bool  avoidClipping = true;
        [SerializeField] private float clipSphereRadius = 0.2f;
        [SerializeField] private LayerMask clipMask = ~0;   

        [Header("Controles")]
        [SerializeField] private KeyCode toggleLockKey = KeyCode.Tab; 

        private float yawRad;     
        private float pitchRad;   
        private float dist;     
        private bool cursorLocked = true;

        // Helpers
        static float Clamp(float v, float a, float b) { if (v < a) return a; if (v > b) return b; return v; }
        static float Deg2Rad(float d) => d * (Utility.MathLite.PI / 180f);
        static float Rad2Deg(float r) => r * (180f / Utility.MathLite.PI);
        static float Lerp(float a, float b, float t) => a + (b - a) * t;

        void Start()
        {
            if (!target)
            {
                var found = GameObject.FindWithTag("Player");
                if (found) target = found.transform;
            }
            dist = Clamp((transform.position - (target ? target.position : transform.position)).magnitude, zoomMin, zoomMax);
            pitchRad = 0f;
            yawRad   = 0f;

            LockCursor(cursorLocked);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleLockKey))
            {
                cursorLocked = !cursorLocked;
                LockCursor(cursorLocked);
            }

            float dt = Time.deltaTime;
            if (cursorLocked)
            {
                float dx = Input.GetAxisRaw("Mouse X");
                float dy = Input.GetAxisRaw("Mouse Y");

                yawRad   += Deg2Rad(yawSpeed   * dx * dt);
                pitchRad -= Deg2Rad(pitchSpeed * dy * dt);

                pitchRad = Clamp(pitchRad, Deg2Rad(minPitchDeg), Deg2Rad(maxPitchDeg));
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (System.Math.Abs(scroll) > 1e-5f)
            {
                dist -= scroll * zoomSpeed;
                dist = Clamp(dist, zoomMin, zoomMax);
            }
        }

        void LateUpdate()
        {
            if (!target) return;

            UQuat qYaw   = UQuat.AxisAngle(new UVec3(1f,0f,0f), 0f); 
            UQuat qPitch = UQuat.AxisAngle(new UVec3(1f,0f,0f), pitchRad);
            UQuat qYawY  = UQuat.AxisAngle(new UVec3(0f,1f,0f), yawRad);
            UQuat orbit  = (qYawY * qPitch).Normalized();

            UVec3 tpos = new UVec3(target.position.x, target.position.y, target.position.z);
            UVec3 desired = tpos + targetOffset + orbit.Rotate(new UVec3(0f, 0f, -dist));

            if (avoidClipping)
            {
                UEVec3 from = new UEVec3(tpos.x + targetOffset.x, tpos.y + targetOffset.y, tpos.z + targetOffset.z);
                UEVec3 to   = new UEVec3(desired.x, desired.y, desired.z);
                UEVec3 dir  = (to - from);
                float  len  = dir.magnitude;
                if (len > 1e-4f)
                {
                    dir /= len;
                    if (Physics.SphereCast(from, clipSphereRadius, dir, out RaycastHit hit, len, clipMask, QueryTriggerInteraction.Ignore))
                    {
                        UEVec3 hitPos = from + dir * (hit.distance - 0.02f);
                        desired = new UVec3(hitPos.x, hitPos.y, hitPos.z);
                    }
                }
            }

            UEVec3 current = transform.position;
            UEVec3 targetP = new UEVec3(desired.x, desired.y, desired.z);
            float t = (followLerp <= 0f) ? 1f : (1f - Mathf.Exp(-followLerp * Time.deltaTime));
            UEVec3 smooth = new UEVec3(
                Lerp(current.x, targetP.x, t),
                Lerp(current.y, targetP.y, t),
                Lerp(current.z, targetP.z, t)
            );

            UVec3 fwd = (new UVec3(tpos.x + targetOffset.x, tpos.y + targetOffset.y, tpos.z + targetOffset.z) -
                         new UVec3(smooth.x, smooth.y, smooth.z)).Normalized();
            float yaw = System.MathF.Atan2(fwd.x, fwd.z);     
            float hyp = (float)System.Math.Sqrt(fwd.x*fwd.x + fwd.z*fwd.z);
            float pitch = System.MathF.Atan2(fwd.y, hyp);   
            UQuat look = (UQuat.AxisAngle(new UVec3(0,1,0), yaw) * UQuat.AxisAngle(new UVec3(1,0,0), -pitch)).Normalized();

            transform.SetPositionAndRotation(smooth, new UEQuat(look.x, look.y, look.z, look.w));
        }

        void LockCursor(bool on)
        {
            Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !on;
        }
    }
}
