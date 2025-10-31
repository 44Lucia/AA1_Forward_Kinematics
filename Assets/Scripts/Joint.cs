using UnityEngine;
using UVec3 = Utility.Vector3;
using UQuat = Utility.Quaternion;

public class Joint : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Limits (degrees)")]
    [SerializeField] private float minAngleDeg = -10f;
    [SerializeField] private float maxAngleDeg = 80f;

    [Header("Local rotation axis (e.g., (-1,0,0) = Vector3.left)")]
    [SerializeField] private UVec3 localAxis = new UVec3(-1f, 0f, 0f);

    [SerializeField, HideInInspector] private UVec3 distanceToNextJoint;

    [SerializeField, HideInInspector] private UQuat baseRotation = UQuat.Identity;
    [SerializeField, HideInInspector] private float currentAngleDeg = 0f;


    public void Initialize(Joint nextJoint)
    {
        if (!nextJoint) return;

        var a = transform.localPosition;
        var b = nextJoint.transform.localPosition;
        distanceToNextJoint = new UVec3(b.x - a.x, b.y - a.y, b.z - a.z);
    }

    public float RotationSpeed
    {
        get => rotationSpeed;
        set => rotationSpeed = value;
    }

    public float MinAngleDeg
    {
        get => minAngleDeg;
        set => minAngleDeg = value;
    }

    public float MaxAngleDeg
    {
        get => maxAngleDeg;
        set => maxAngleDeg = value;
    }

    public UVec3 LocalAxis
    {
        get => localAxis;
        set => localAxis = value;
    }

    public UVec3 DistanceToNextJoint => distanceToNextJoint;

    public UQuat BaseRotation
    {
        get => baseRotation;
        set => baseRotation = value;
    }

    public float CurrentAngleDeg
    {
        get => currentAngleDeg;
        set => currentAngleDeg = value;
    }
}
