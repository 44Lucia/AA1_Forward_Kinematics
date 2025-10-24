using UnityEngine;

public class Joint : MonoBehaviour
{
    [field: SerializeField] public float RotationSpeed { get; set; } = 30f;
    public Vector3 DistanceToNextJoint { get; private set; }

    public void Initialize(Joint p_nextJoint)
    {
        if (!p_nextJoint) return;

        DistanceToNextJoint = p_nextJoint.transform.localPosition - transform.localPosition;
    }
}