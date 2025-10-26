using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MagnetGrabber : MonoBehaviour
{
    [SerializeField] private KeyCode _toggleKey = KeyCode.F;
    [SerializeField] private LayerMask _grabbableMask = ~0;   
    [SerializeField] private float _breakForce  = Mathf.Infinity;
    [SerializeField] private float _breakTorque = Mathf.Infinity;
    [SerializeField] private bool _requireNonKinematic = true; 
    [SerializeField] private bool _useAutoAnchors = true;     

    private FixedJoint _currentJoint;     
    private Rigidbody  _grabbedBody;     
    private readonly HashSet<Rigidbody> _contacts = new(); 

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnEnable()
    {
        _contacts.Clear();
        _currentJoint = null;
        _grabbedBody  = null;
    }

    private void OnDisable()
    {
        Release();
        _contacts.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            if (_currentJoint != null) { Release(); }
            else { TryAttachFromContacts(); }
        }
    }

    private void OnCollisionEnter(Collision c)
    {
        Rigidbody rb = c.rigidbody;
        if (!rb) return;
        if (!IsGrabbable(rb)) return;
        _contacts.Add(rb);
    }

    private void OnCollisionStay(Collision c)
    {
        Rigidbody rb = c.rigidbody;
        if (!rb) return;
        if (!IsGrabbable(rb)) return;
        _contacts.Add(rb);
    }

    private void OnCollisionExit(Collision c)
    {
        Rigidbody rb = c.rigidbody;
        if (!rb) return;
        _contacts.Remove(rb);
    }

    private bool IsGrabbable(Rigidbody rb)
    {
        if (((1 << rb.gameObject.layer) & _grabbableMask) == 0) return false;
        if (_requireNonKinematic && rb.isKinematic) return false;

        if (rb == GetComponent<Rigidbody>()) return false;
        return true;
    }

    private void TryAttachFromContacts()
    {
        if (_currentJoint != null) return;

        foreach (Rigidbody rb in _contacts)
        {
            if (!rb) continue;
            if (!IsGrabbable(rb)) continue;

            FixedJoint fj = gameObject.AddComponent<FixedJoint>();
            fj.connectedBody = rb;
            fj.breakForce    = _breakForce;
            fj.breakTorque   = _breakTorque;
            fj.enablePreprocessing = false;

            fj.autoConfigureConnectedAnchor = _useAutoAnchors;

            _currentJoint = fj;
            _grabbedBody  = rb;
            return;
        }
    }

    private void Release()
    {
        if (_currentJoint != null) {
            Destroy(_currentJoint);
            _currentJoint = null;
        }
        _grabbedBody = null;
    }

    private void OnJointBreak(float breakForce)
    {
        _currentJoint = null;
        _grabbedBody  = null;
    }
}
