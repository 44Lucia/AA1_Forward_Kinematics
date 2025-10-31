using UnityEngine;

// Alias a nuestras libs
using MyVec3 = Utility.Vector3;
using MyQuat = Utility.Quaternion;

public class MyRobotController : MonoBehaviour
{
    [Header("Joints (mínimo 3)")]
    [SerializeField] private Joint joint1;
    [SerializeField] private Joint joint2;
    [SerializeField] private Joint joint3;

    private Joint[] joints;
    private int currentJointIndex = 0;
    private float yawInput = 0f;
    private float joint3YawDeg = 0f;


    void Start()
    {
        joints = new[] { joint1, joint2, joint3 };

        joint1.Initialize(joint2);
        joint2.Initialize(joint3);

        foreach (Joint j in joints)
        {
            j.BaseRotation = (MyQuat)j.transform.rotation;
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

        // Horizontal entry: ← / →
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            yawInput = -1f;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            yawInput = 1f;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            yawInput = 0f;
        }

        // Update joint3 yaw angle
        if (currentJointIndex == 2 && System.Math.Abs(yawInput) > 1e-6f)
        {
            joint3YawDeg += yawInput * joint3.RotationSpeed * Time.deltaTime;
        }
    }

    void ApplyLimitedRotation()
    {
        // Vertical entry: ↑ / ↓
        float vertical =
            (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) +
            (Input.GetKey(KeyCode.DownArrow) ? -1f : 0f);

        if (System.Math.Abs(vertical) < 1e-6f && currentJointIndex != 2) return;

        Joint j = joints[currentJointIndex];

        // Δangle in degrees this frame
        float deltaDeg = j.RotationSpeed * Time.deltaTime * vertical;

        // Clamp to the allowed range (use it from your libs)
        float targetDeg = Utility.MathLite.Clamp(
            j.CurrentAngleDeg + deltaDeg, j.MinAngleDeg, j.MaxAngleDeg);

        j.CurrentAngleDeg = targetDeg;

        // Rotation = base * AxisAngle(localaxis, currentAngle)
        float angleRad = Utility.MathLite.Deg2Rad(j.CurrentAngleDeg);
        MyQuat q = (j.BaseRotation * MyQuat.AxisAngle(j.LocalAxis, angleRad)).Normalized();

        // For joint3, add yaw rotation
        if (j == joint3)
        {
            MyQuat yawQuat = MyQuat.AxisAngle(MyVec3.Up, Utility.MathLite.Deg2Rad(joint3YawDeg));
            q = (q * yawQuat).Normalized();
        }

        // Add rotation of the robot itself
        q = ((MyQuat)transform.rotation * q).Normalized();

        j.transform.rotation = (Quaternion)q;
    }

    void ForwardKinematics()
    {
        // Joint 1: base rotation
        MyQuat r1 = (MyQuat)transform.rotation * joint1.BaseRotation * MyQuat.AxisAngle(joint1.LocalAxis, Utility.MathLite.Deg2Rad(joint1.CurrentAngleDeg));
        joint1.transform.rotation = (Quaternion)r1;

        // Joint 2: relative to joint 1
        MyQuat r2 = r1 * joint2.BaseRotation * MyQuat.AxisAngle(joint2.LocalAxis, Utility.MathLite.Deg2Rad(joint2.CurrentAngleDeg));
        joint2.transform.rotation = (Quaternion)r2;

        // Joint 3: relative to joint 2, plus yaw
        MyQuat r3 = r2 * joint3.BaseRotation * MyQuat.AxisAngle(joint3.LocalAxis, Utility.MathLite.Deg2Rad(joint3.CurrentAngleDeg));
        MyQuat yawQuat = MyQuat.AxisAngle(MyVec3.Up, Utility.MathLite.Deg2Rad(joint3YawDeg));
        r3 = r3 * yawQuat;
        joint3.transform.rotation = (Quaternion)r3;

        // Use joint1's current position as the base for the chain:
        MyVec3 p1 = (MyVec3)joint1.transform.position;
        MyVec3 p2 = p1 + r1.Rotate(joint1.DistanceToNextJoint);
        joint2.transform.position = (Vector3)p2;

        MyVec3 p3 = p2 + r2.Rotate(joint2.DistanceToNextJoint);
        joint3.transform.position = (Vector3)p3;
    }
}