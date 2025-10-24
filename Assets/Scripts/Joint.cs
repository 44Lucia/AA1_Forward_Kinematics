using UnityEngine;

public class Joint : MonoBehaviour
{
    [field: SerializeField] public float RotationSpeed { get; set; } = 30f;
    public Vector3 DistanceToNextJoint { get; private set; }

    private LineRenderer lineRenderer;
    private Joint nextJoint;
    [SerializeField] private float angleOffset;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    public void Initialize(Joint p_nextJoint)
    {
        if (!p_nextJoint) return;

        DistanceToNextJoint = p_nextJoint.transform.position - transform.position;

        nextJoint = p_nextJoint;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, p_nextJoint.transform.position);
    }

    private void Update()
    {
        lineRenderer.SetPosition(0, transform.position);

        if (!nextJoint) return;
        lineRenderer.SetPosition(1, nextJoint.transform.position);
    }
}