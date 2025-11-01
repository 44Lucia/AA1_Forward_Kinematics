using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class MagnetGrabber : MonoBehaviour
{
    [FormerlySerializedAs("_toggleKey")] [SerializeField]
    private KeyCode toggleKey = KeyCode.Space;

    [FormerlySerializedAs("_grabbableMask")] [SerializeField]
    private LayerMask grabbableMask = ~0;

    [FormerlySerializedAs("_breakForce")] [SerializeField]
    private float breakForce = Mathf.Infinity;

    [FormerlySerializedAs("_breakTorque")] [SerializeField]
    private float breakTorque = Mathf.Infinity;

    [FormerlySerializedAs("_requireNonKinematic")] [SerializeField]
    private bool requireNonKinematic = true;

    [FormerlySerializedAs("_useAutoAnchors")] [SerializeField]
    private bool useAutoAnchors = true;

    private FixedJoint currentJoint;
    private readonly HashSet<Rigidbody> contacts = new();

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnEnable()
    {
        contacts.Clear();
        currentJoint = null;
    }

    private void OnDisable()
    {
        Release();
        contacts.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (currentJoint)
            {
                Release();
            }
            else
            {
                TryAttachFromContacts();
            }
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        Rigidbody rb = c.attachedRigidbody;
        if (!rb) return;
        if (!IsGrabbable(rb)) return;
        contacts.Add(rb);
    }

    private void OnTriggerStay(Collider c)
    {
        Rigidbody rb = c.attachedRigidbody;
        if (!rb) return;
        if (!IsGrabbable(rb)) return;
        contacts.Add(rb);
    }

    private void OnTriggerExit(Collider c)
    {
        Rigidbody rb = c.attachedRigidbody;
        if (!rb) return;
        contacts.Remove(rb);
    }

    private bool IsGrabbable(Rigidbody rb)
    {
        if (((1 << rb.gameObject.layer) & grabbableMask) == 0) return false;
        if (requireNonKinematic && rb.isKinematic) return false;

        if (rb == GetComponent<Rigidbody>()) return false;
        return true;
    }

    private void TryAttachFromContacts()
    {
        if (currentJoint != null) return;

        foreach (Rigidbody rb in contacts)
        {
            if (!rb) continue;
            if (!IsGrabbable(rb)) continue;

            FixedJoint fj = gameObject.AddComponent<FixedJoint>();
            fj.connectedBody = rb;
            fj.breakForce = breakForce;
            fj.breakTorque = breakTorque;
            fj.enablePreprocessing = false;

            fj.autoConfigureConnectedAnchor = useAutoAnchors;

            currentJoint = fj;
            return;
        }
    }

    private void Release()
    {
        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }
    }

    private void OnJointBreak(float breakForce)
    {
        currentJoint = null;
    }
}