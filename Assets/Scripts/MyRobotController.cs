using UnityEngine;

public class ForwardKinematics2D : MonoBehaviour
{
    [SerializeField] private Joint joint1;
    [SerializeField] private Joint joint2;
    [SerializeField] private Joint joint3;

    private void Start()
    {
        joint1.Initialize(joint2);
        joint2.Initialize(joint3);

        joint1.transform.LookAt(joint2.transform);
        joint2.transform.LookAt(joint3.transform);
    }

    void Update()
    {
        //joint1.transform.Rotate(Vector3.left, joint1.RotationSpeed * Time.deltaTime * Input.GetAxis("Vertical"));

        //joint2.transform.position = joint1.transform.position + joint1.transform.rotation * joint1.DistanceToNextJoint;

        //joint2.transform.Rotate(Vector3.left, joint2.RotationSpeed * Time.deltaTime * Input.GetAxis("Horizontal"));
    }
}