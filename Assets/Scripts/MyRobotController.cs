using UnityEngine;

// Alias a nuestras libs
using UVec3 = Utility.Vector3;
using UQuat = Utility.Quaternion;
using UEVec3 = UnityEngine.Vector3;
using UEQuat = UnityEngine.Quaternion;

public class MyRobotController : MonoBehaviour
{
    [Header("Joints (mínimo 3)")]
    [SerializeField] private Joint joint1;
    [SerializeField] private Joint joint2;
    [SerializeField] private Joint joint3;

    private Joint[] joints;
    private int currentJointIndex = 0;

    static UVec3 FromU(UEVec3 v) => new UVec3(v.x, v.y, v.z);
    static UQuat FromU(UEQuat q) => new UQuat(q.x, q.y, q.z, q.w).Normalized();
    static UEVec3 ToU(UVec3 v) => new UEVec3(v.x, v.y, v.z);
    static UEQuat ToU(UQuat q) => new UEQuat(q.x, q.y, q.z, q.w);

    void Start()
    {
        joints = new[] { joint1, joint2, joint3 };

        joint1.Initialize(joint2);
        joint2.Initialize(joint3);

        foreach (Joint j in joints)
        {
            j.BaseRotation = FromU(j.transform.rotation);
            j.CurrentAngleDeg = 0f;
        }
    }

    void Update()
    {
        HandleInput();
        ApplyLimitedRotation();
        ForwardKinematics();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            currentJointIndex = (currentJointIndex + 1) % joints.Length;
    }

    void ApplyLimitedRotation()
    {
        // Vertical entry: ↑ / ↓
        float vertical =
            (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) +
            (Input.GetKey(KeyCode.DownArrow) ? -1f : 0f);

        if (System.Math.Abs(vertical) < 1e-6f) return;

        Joint j = joints[currentJointIndex];

        // Δangle in degrees this frame
        float deltaDeg = j.RotationSpeed * Time.deltaTime * vertical;

        // Clamp to the allowed range (use it from your libs)
        float targetDeg = Utility.MathLite.Clamp(
            j.CurrentAngleDeg + deltaDeg, j.MinAngleDeg, j.MaxAngleDeg);

        j.CurrentAngleDeg = targetDeg;

        // Rotation = base * AxisAngle(localaxis, currentAngle)
        float angleRad = Utility.MathLite.Deg2Rad(j.CurrentAngleDeg);
        UQuat q = (j.BaseRotation * UQuat.AxisAngle(j.LocalAxis, angleRad)).Normalized();
        j.transform.rotation = ToU(q);
    }

    void ForwardKinematics()
    {
        // Repositions child joints based on the current pose (FK without hierarchy)
        // J2 depends on J1
        UVec3 p1 = FromU(joint1.transform.position);
        UQuat r1 = FromU(joint1.transform.rotation);
        UVec3 p2 = p1 + r1.Rotate(joint1.DistanceToNextJoint);
        joint2.transform.position = ToU(p2);

        // J3 depends on J2
        UQuat r2 = FromU(joint2.transform.rotation);
        UVec3 p3 = p2 + r2.Rotate(joint2.DistanceToNextJoint);
        joint3.transform.position = ToU(p3);
    }
}
