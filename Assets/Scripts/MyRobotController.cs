using UnityEngine;
using UnityEngine.InputSystem;

public class MyRobotController : MonoBehaviour
{
    [Header("Joints"), SerializeField] private Joint joint1;
    [SerializeField] private Joint joint2;
    [SerializeField] private Joint joint3;

    private readonly Keyboard keyboard = Keyboard.current;
    private int currentJointIndex = 0;

    private void Start()
    {
        joint1.Initialize(joint2);
        joint2.Initialize(joint3);
    }

    private void Update()
    {
        HandleInput();
        HandleJoints();
    }

    private void HandleInput()
    {
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            currentJointIndex = (currentJointIndex + 1) % 3;
        }
    }

    private void HandleJoints()
    {
        float verticalInput = keyboard.upArrowKey.isPressed ? 1f : keyboard.downArrowKey.isPressed ? -1f : 0f;
        switch (currentJointIndex)
        {
            case 0:
                joint1.transform.Rotate(Vector3.left, joint1.RotationSpeed * Time.deltaTime * verticalInput);
                break;
            case 1:
                joint2.transform.Rotate(Vector3.left, joint2.RotationSpeed * Time.deltaTime * verticalInput);
                break;
            case 2:
                joint3.transform.Rotate(Vector3.left, joint3.RotationSpeed * Time.deltaTime * verticalInput);
                break;
        }

        joint2.transform.position = joint1.transform.position + joint1.transform.rotation * joint1.DistanceToNextJoint;
        joint3.transform.position = joint2.transform.position + joint2.transform.rotation * joint2.DistanceToNextJoint;
    }
}